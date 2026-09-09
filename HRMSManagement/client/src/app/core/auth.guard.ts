import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { PermissionsService } from './permissions.service';

export const authGuard: CanActivateFn = () => {
  const token = localStorage.getItem(environment.tokenKey);
  if (token) return true;

  const router = inject(Router);
  return router.parseUrl('/login');
};

export function permissionGuard(requiredPermission: string): CanActivateFn {
  return () => {
    const token = localStorage.getItem(environment.tokenKey);
    const router = inject(Router);
    if (!token) return router.parseUrl('/login');

    const permissions = inject(PermissionsService);
    return permissions.has(requiredPermission) ? true : router.parseUrl('/dashboard');
  };
}
