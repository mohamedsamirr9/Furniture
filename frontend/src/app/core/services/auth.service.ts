import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import {
  BehaviorSubject,
  Observable,
  catchError,
  tap,
  throwError
} from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponseDto,
  LoginDto,
  RegisterDto,
  UserDto,
  UpdateProfileDto,
  ChangePasswordDto,
  BecomeSellerDto,
  ResetPasswordDto
} from '../models/auth.model';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = `${environment.apiUrl}/Account`;

  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {
    this.initializeFromStorage();
  }

  private initializeFromStorage(): void {
    const savedUser = localStorage.getItem('user');
    const token = localStorage.getItem('token');

    if (savedUser) {
      this.currentUserSubject.next(JSON.parse(savedUser));
      this.notificationService.restoreFromStorage();
    } else {
      this.notificationService.clearNotificationsState();
    }

    if (token) {
      this.notificationService.startConnection(token).then(() => {
        this.notificationService.loadNotifications();
      });
    }
  }

  get token(): string | null {
    return localStorage.getItem('token');
  }

  get refreshTokenValue(): string | null {
    return localStorage.getItem('refreshToken');
  }

  get currentUserId(): string | null {
    const fromUser = this.currentUserSubject.value?.id;
    if (fromUser) return fromUser;

    const decoded = this.decodeToken();
    if (!decoded) return null;

    return (
      decoded['nameid'] ||
      decoded['ClaimTypes.NameIdentifier'] ||
      decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      null
    );
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

    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<AuthResponseDto>(`${this.baseUrl}/refresh`, { token: refreshToken }).pipe(
      tap((response) => this.setSession(response)),
      catchError((err: any) => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const refreshToken = this.refreshTokenValue;

    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);

    this.notificationService.stopConnection().then(() => {
      this.notificationService.clearNotificationsState();
    });

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

  isLoggedIn(): boolean {
    return this.isTokenValid();
  }

  getUserRole(): string | null {
    try {
      const decoded: any = this.decodeToken();
      if (!decoded) return null;

      const role =
        decoded['role'] ||
        decoded['ClaimTypes.Role'] ||
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      return role ? role.toLowerCase() : null;
    } catch (e) {
      console.error('Failed to decode token', e);
      return null;
    }
  }

  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/admin/users`);
  }

  private setSession(authResult: any): void {
    const token = authResult.token || authResult.Token;
    const refreshToken = authResult.refreshToken || authResult.RefreshToken;
    const user = authResult.user || authResult.User;

    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSubject.next(user);
      this.notificationService.restoreFromStorage();
    }

    if (token) {
      localStorage.setItem('token', token);
      this.notificationService.startConnection(token).then(() => {
        this.notificationService.loadNotifications();
      });
    }

    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
    }
  }

  private decodeToken(): any | null {
    const token = this.token;
    if (!token) return null;

    try {
      return jwtDecode<any>(token);
    } catch {
      return null;
    }
  }

  private isTokenValid(): boolean {
    const decoded = this.decodeToken();
    if (!decoded) return false;

    const exp: number | undefined = decoded['exp'];
    if (!exp) return true;

    const nowSeconds = Math.floor(Date.now() / 1000);
    return exp > nowSeconds;
  }
}