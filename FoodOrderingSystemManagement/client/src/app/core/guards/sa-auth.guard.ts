import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../authentication/auth.service';

/** Protects SuperAdmin routes — must be authenticated as SuperAdmin. */
export const saAuthGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/sa/login']);
    return false;
  }

  if (!auth.isSuperAdminUser()) {
    router.navigate(['/auth/login']);
    return false;
  }

  return true;
};
