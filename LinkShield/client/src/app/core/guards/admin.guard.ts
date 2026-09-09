import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthApiService } from '../services/auth-api.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthApiService);
  const router = inject(Router);

  if (auth.hasAnyRole('SuperAdmin', 'Admin')) return true;

  return router.createUrlTree(['/auth/login']);
};
