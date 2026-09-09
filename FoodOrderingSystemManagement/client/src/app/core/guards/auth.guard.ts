import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../authentication/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }

  // SuperAdmin must use the SA panel, not the restaurant app
  if (authService.isSuperAdminUser()) {
    router.navigate(['/sa/organizations']);
    return false;
  }

  return true;
};
