import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { PermissionsService } from './permissions.service';
import { PATIENT_TOKEN_KEY } from './patient-auth.service';

export const authGuard: CanActivateFn = () => {
  const token = localStorage.getItem(environment.tokenKey);
  if (token) return true;

  const router = inject(Router);
  return router.parseUrl('/login');
};

export const patientAuthGuard: CanActivateFn = () => {
  const token = localStorage.getItem(PATIENT_TOKEN_KEY);
  if (token) return true;

  const router = inject(Router);
  return router.parseUrl('/patient/login');
};

export function permissionGuard(requiredPermission: string): CanActivateFn {
  return () => {
    const token = localStorage.getItem(environment.tokenKey);
    const router = inject(Router);
    if (!token) return router.parseUrl('/login');

    const permissions = inject(PermissionsService);
    return permissions.has(requiredPermission) ? true : router.parseUrl('/medicines');
  };
}
