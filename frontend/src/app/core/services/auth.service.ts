import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { BehaviorSubject, Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponseDto,
  LoginDto,
  RegisterDto,
  UserDto,
  RefreshTokenDto,
  UpdateProfileDto,
  ChangePasswordDto,
  BecomeSellerDto,
  ResetPasswordDto
} from '../models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = `${environment.apiUrl}/Account`;
  
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      this.currentUserSubject.next(JSON.parse(savedUser));
    }
  }

  get token(): string | null {
    return localStorage.getItem('token');
  }

  get refreshTokenValue(): string | null {
    return localStorage.getItem('refreshToken');
  }

  register(data: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/register`, data);
  }

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, credentials).pipe(
      tap((response) => this.setSession(response))
    );
  }

  refreshToken(): Observable<AuthResponseDto> {
    const refreshToken = this.refreshTokenValue;
    if (!refreshToken) return throwError(() => new Error('No refresh token available'));

    return this.http.post<AuthResponseDto>(`${this.baseUrl}/refresh`, { token: refreshToken }).pipe(
      tap((response) => this.setSession(response)),
      catchError((err: any) => {
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const refreshToken = this.refreshTokenValue;
    
    // Clear state FIRST to prevent loops if revoke triggers 401
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);

    // Only call revoke if we had a refresh token
    if (refreshToken) {
      this.http.post(`${this.baseUrl}/revoke?refreshToken=${refreshToken}`, {}).subscribe({
        error: (err: any) => console.error('Token revocation failed', err)
      });
    }
  }

  getCurrentUser(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.baseUrl}/me`).pipe(
      tap((user) => {
        this.currentUserSubject.next(user);
        localStorage.setItem('user', JSON.stringify(user));
      })
    );
  }

  updateProfile(data: UpdateProfileDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.baseUrl}/profile`, data).pipe(
      tap((user) => {
        this.currentUserSubject.next(user);
        localStorage.setItem('user', JSON.stringify(user));
      })
    );
  }

  changePassword(data: ChangePasswordDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/change-password`, data);
  }

  becomeSeller(data: BecomeSellerDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/become-seller`, data);
  }

  sendOtp(email: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/send-otp`, { email });
  }

  verifyOtp(email: string, otp: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/verify-otp`, { email, otp });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(data: ResetPasswordDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/reset-password`, data);
  }

  private setSession(authResult: any): void {
    // Handle both camelCase and PascalCase from API
    const token = authResult.token || authResult.Token;
    const refreshToken = authResult.refreshToken || authResult.RefreshToken;
    const user = authResult.user || authResult.User;

    if (token) localStorage.setItem('token', token);
    if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSubject.next(user);
    }
  }

  isLoggedIn(): boolean {
    const hasToken = !!this.token;
    const hasUser = !!this.currentUserSubject.value;
    return hasToken && hasUser;
  }

  getUserRole(): string | null {
    const token = this.token;
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      // Standard ASP.NET Core Role claim key
      const role = decoded['role'] || 
                   decoded['ClaimTypes.Role'] || 
                   decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      
      return role ? role.toLowerCase() : null;
    } catch (e) {
      console.error('Failed to decode token', e);
      return null;
    }
  }
}
