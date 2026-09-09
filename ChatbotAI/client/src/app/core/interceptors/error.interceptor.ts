import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ApiErrorResponse } from '../models/api-error.model';

const SILENT_STATUSES = new Set([401]);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && !SILENT_STATUSES.has(error.status)) {
        const body = error.error as ApiErrorResponse | undefined;
        const message = body?.message ?? 'Something went wrong. Please try again.';
        snackBar.open(message, 'Dismiss', { duration: 5000, panelClass: 'app-snackbar-error' });
      }
      return throwError(() => error);
    })
  );
};
