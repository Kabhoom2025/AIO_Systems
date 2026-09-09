import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { environment } from '../../environments/environment';

export const authGuard: CanActivateFn = () => {
  const token = localStorage.getItem(environment.tokenKey);
  if (token) return true;

  const router = inject(Router);
  return router.parseUrl('/login');
};
