import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../authentication/auth.service';

/**
 * Route guard that checks whether the current user's organization
 * has at least one of the required platform modules enabled.
 *
 * Super-admin users (isSuperAdmin = true) always pass.
 * Orgs with no modules assigned (legacy / newly created) always pass
 * so existing users are never accidentally locked out.
 */
export const moduleGuard = (requiredModules: string[]): CanActivateFn => () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  // Super-admin has access to everything
  if (auth.isSuperAdmin()) return true;

  const orgModules = auth.enabledModules();

  // No modules assigned = legacy org, allow all (backward compatibility)
  if (orgModules.length === 0) return true;

  const hasAccess = requiredModules.some(m => orgModules.includes(m));
  if (!hasAccess) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
