import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const token = authService.token;
  const isApiUrl = req.url.startsWith(environment.apiUrl);
  
  // Specific exclusions for login and register
  const isAuthRequest = req.url.includes('/Account/login') || req.url.includes('/Account/register');
  const isRefreshRequest = req.url.includes('/Account/refresh');

  // Add auth header if token exists and request is to API and not an auth request that manages its own tokens
  if (token && isApiUrl && !isAuthRequest) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && isApiUrl && !isAuthRequest) {
        // If it's already a refresh request or we have no refresh token, just return error
        if (isRefreshRequest || !authService.refreshTokenValue) {
          return throwError(() => error);
        }
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService) => {
  return authService.refreshToken().pipe(
    switchMap((response: any) => {
      // Handle both camelCase and PascalCase
      const newToken = response.token || response.Token;
      const clonedReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${newToken}`
        }
      });
      return next(clonedReq);
    }),
    catchError((error: any) => {
      // If refresh fails, just report the error. User-initiated logout is now enforced.
      return throwError(() => error);
    })
  );
};
