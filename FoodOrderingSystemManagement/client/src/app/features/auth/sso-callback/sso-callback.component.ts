import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/authentication/auth.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { getHomeRoute } from '../../../core/guards/role.guard';
import { APP_CONSTANTS } from '../../../core/constants/app.constants';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sso-callback',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="sso-loading">
      <mat-progress-spinner diameter="48" mode="indeterminate"></mat-progress-spinner>
      <p>Completing sign-in…</p>
    </div>
  `,
  styles: [`
    .sso-loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      gap: 20px;
      color: #666;
      font-size: .95rem;
    }
  `],
})
export class SsoCallbackComponent implements OnInit {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private auth   = inject(AuthService);
  private notify = inject(NotificationService);

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const token  = params.get('token');
    const name   = params.get('name') ?? '';
    const role   = params.get('role') ?? '';

    if (!token) {
      this.notify.error('SSO sign-in failed. No token received.');
      this.router.navigate(['/auth/login']);
      return;
    }

    // Decode the JWT to get the full user payload (same structure as regular login)
    try {
      const payload  = JSON.parse(atob(token.split('.')[1]));
      const loginRes = {
        token,
        name,
        email:            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? '',
        roleName:         role,
        userId:           +(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? 0),
        roleId:           0,
        organizationId:   payload['organizationId'] ? +payload['organizationId'] : null,
        organizationName: null,
        isSuperAdmin:     role === 'SuperAdmin',
        expiresAt:        new Date(payload.exp * 1000).toISOString(),
      };

      localStorage.setItem(APP_CONSTANTS.TOKEN_KEY, token);
      localStorage.setItem(APP_CONSTANTS.USER_KEY, JSON.stringify(loginRes));
      localStorage.setItem('organizationId',   String(loginRes.organizationId ?? null));
      localStorage.setItem('organizationName', loginRes.organizationName ?? '');
      localStorage.setItem('isSuperAdmin',     String(loginRes.isSuperAdmin));

      this.auth.reloadUser();
      this.notify.success(`Welcome, ${name}!`);
      this.router.navigate([getHomeRoute(role)]);
    } catch {
      this.notify.error('SSO sign-in failed. Invalid token.');
      this.router.navigate(['/auth/login']);
    }
  }
}
