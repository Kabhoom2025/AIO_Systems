import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/authentication/auth.service';

@Component({
  selector: 'app-sa-login',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatProgressSpinnerModule,
  ],
  template: `
    <div class="sa-login-wrap">
      <div class="sa-login-card">
        <div class="sa-brand">
          <div class="sa-logo">
            <mat-icon>admin_panel_settings</mat-icon>
          </div>
          <h1>Super Admin</h1>
          <p>Organization Control Panel</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="sa-form">
          @if (error()) {
            <div class="sa-error">
              <mat-icon>error_outline</mat-icon>
              <span>{{ error() }}</span>
            </div>
          }

          <mat-form-field appearance="outline" class="sa-field">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" placeholder="admin@example.com" autocomplete="username" />
            <mat-icon matPrefix>email</mat-icon>
            @if (form.get('email')?.hasError('required') && form.get('email')?.touched) {
              <mat-error>Email is required</mat-error>
            }
            @if (form.get('email')?.hasError('email') && form.get('email')?.touched) {
              <mat-error>Enter a valid email</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="sa-field">
            <mat-label>Password</mat-label>
            <input matInput [type]="hide() ? 'password' : 'text'" formControlName="password" autocomplete="current-password" />
            <mat-icon matPrefix>lock</mat-icon>
            <button mat-icon-button matSuffix type="button" (click)="hide.set(!hide())">
              <mat-icon>{{ hide() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (form.get('password')?.hasError('required') && form.get('password')?.touched) {
              <mat-error>Password is required</mat-error>
            }
          </mat-form-field>

          <button mat-raised-button color="primary" type="submit" class="sa-submit" [disabled]="loading()">
            @if (loading()) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              <ng-container>
                <mat-icon>login</mat-icon>
                Sign In
              </ng-container>
            }
          </button>
        </form>

        <p class="sa-back">
          <a routerLink="/auth/login">
            <mat-icon>arrow_back</mat-icon> Back to Restaurant Login
          </a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .sa-login-wrap {
      min-height: 100vh;
      background: linear-gradient(135deg, #0d1b2a 0%, #1a1a2e 50%, #16213e 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
    }

    .sa-login-card {
      background: #fff;
      border-radius: 20px;
      padding: 48px 40px 36px;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 24px 64px rgba(0,0,0,.4);
    }

    .sa-brand {
      text-align: center;
      margin-bottom: 36px;
    }

    .sa-logo {
      width: 72px;
      height: 72px;
      background: linear-gradient(135deg, #1a237e, #311b92);
      border-radius: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 16px;
      box-shadow: 0 8px 24px rgba(49,27,146,.35);

      mat-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;
        color: #fff;
      }
    }

    h1 {
      margin: 0 0 6px;
      font-size: 26px;
      font-weight: 700;
      color: #1a237e;
    }

    p {
      margin: 0;
      font-size: 14px;
      color: #757575;
    }

    .sa-form { display: flex; flex-direction: column; gap: 4px; }

    .sa-error {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      background: #ffebee;
      border: 1px solid #ffcdd2;
      border-radius: 10px;
      color: #c62828;
      font-size: 14px;
      margin-bottom: 8px;

      mat-icon { font-size: 18px; width: 18px; height: 18px; flex-shrink: 0; }
    }

    .sa-field { width: 100%; }

    .sa-submit {
      margin-top: 8px;
      height: 48px;
      font-size: 15px;
      font-weight: 600;
      border-radius: 12px !important;
      background: linear-gradient(135deg, #1a237e, #311b92) !important;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;

      mat-icon { font-size: 20px; width: 20px; height: 20px; }
      mat-spinner { --mdc-circular-progress-active-indicator-color: #fff !important; }
    }

    .sa-back {
      text-align: center;
      margin: 20px 0 0;

      a {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: #757575;
        font-size: 13px;
        text-decoration: none;
        transition: color .15s;

        mat-icon { font-size: 16px; width: 16px; height: 16px; }
        &:hover { color: #1a237e; }
      }
    }
  `],
})
export class SaLoginComponent {
  form: FormGroup;
  loading = signal(false);
  hide    = signal(true);
  error   = signal('');

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
  ) {
    if (this.auth.isAuthenticated() && this.auth.isSuperAdminUser()) {
      this.router.navigate(['/sa/organizations']);
    }
    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set('');
    const { email, password } = this.form.value;
    this.auth.login({ email, password }).subscribe({
      next: res => {
        this.loading.set(false);
        if (res.success) {
          if (!res.data.isSuperAdmin) {
            this.auth.logout();
            this.error.set('This account does not have Super Admin privileges.');
            return;
          }
          this.router.navigate(['/sa/organizations']);
        } else {
          this.error.set(res.message || 'Login failed.');
        }
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Invalid email or password.');
      },
    });
  }
}
