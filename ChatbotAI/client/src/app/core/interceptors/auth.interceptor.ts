import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_FREE_PATHS = ['/auth/login', '/auth/register', '/auth/refresh'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const accessToken = authService.getAccessToken();

  const isAuthFree = AUTH_FREE_PATHS.some((path) => req.url.includes(path));
  const authorizedRequest = accessToken && !isAuthFree
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthFree) {
        return authService.refresh().pipe(
          switchMap((refreshed) => {
            if (!refreshed) {
              authService.logout();
              return throwError(() => error);
            }
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${refreshed.accessToken}` } });
            return next(retried);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
