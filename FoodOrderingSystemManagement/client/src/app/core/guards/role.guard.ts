import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../authentication/auth.service';
import { APP_CONSTANTS } from '../constants/app.constants';

/** Returns the landing page route for a given role. */
export function getHomeRoute(role: string): string {
  switch (role) {
    case 'SuperAdmin':                          return '/sa/organizations';
    case APP_CONSTANTS.ROLES.WAITER:            return '/waiter';
    case APP_CONSTANTS.ROLES.INVENTORY_MANAGER: return '/inventory';
    default:                                    return '/';
  }
}

export const roleGuard = (allowedRoles: string[]): CanActivateFn => () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }

  if (!allowedRoles.includes(auth.userRole())) {
    router.navigate([getHomeRoute(auth.userRole())]);
    return false;
  }

  return true;
};

/**
 * Guard for the dashboard route — redirects roles that have a dedicated
 * home page away from the generic dashboard immediately.
 */
export const dashboardGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  const role   = auth.userRole();

  if (role === APP_CONSTANTS.ROLES.WAITER || role === APP_CONSTANTS.ROLES.INVENTORY_MANAGER) {
    router.navigate([getHomeRoute(role)]);
    return false;
  }
  return true;
};
