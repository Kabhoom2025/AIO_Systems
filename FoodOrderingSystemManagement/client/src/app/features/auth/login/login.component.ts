import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/authentication/auth.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { getHomeRoute } from '../../../core/guards/role.guard';
import { environment } from '../../../../environments/environment';

interface SsoProvider {
  id: string;
  label: string;
  icon: string; // inline SVG path data
  color: string;
  bg: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatTooltipModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit {
  readonly currentYear = new Date().getFullYear();
  readonly theme       = inject(ThemeService);
  loginForm: FormGroup;
  loading      = signal(false);
  hidePassword = signal(true);
  ssoLoading   = signal<string | null>(null);

  readonly ssoProviders: SsoProvider[] = [
    {
      id: 'google', label: 'Google', color: '#ea4335', bg: '#fff',
      icon: 'M12.545 10.239v3.821h5.445c-.712 2.315-2.647 3.972-5.445 3.972a6.033 6.033 0 1 1 0-12.064c1.498 0 2.866.549 3.921 1.453l2.814-2.814A9.969 9.969 0 0 0 12.545 2C7.021 2 2.543 6.477 2.543 12s4.478 10 10.002 10c8.396 0 10.249-7.85 9.426-11.748l-9.426-.013z',
    },
    {
      id: 'microsoft', label: 'Microsoft', color: '#00a4ef', bg: '#fff',
      icon: 'M11.5 2H2v9.5h9.5V2zm0 10.5H2V22h9.5v-9.5zm10.5 0h-9.5V22H22v-9.5zM22 2h-9.5v9.5H22V2z',
    },
    {
      id: 'github', label: 'GitHub', color: '#24292e', bg: '#fff',
      icon: 'M12 0C5.374 0 0 5.373 0 12c0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23A11.509 11.509 0 0 1 12 5.803c1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576C20.566 21.797 24 17.3 24 12c0-6.627-5.373-12-12-12z',
    },
    {
      id: 'facebook', label: 'Facebook', color: '#1877f2', bg: '#fff',
      icon: 'M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z',
    },
  ];

  private apiBase = environment.apiUrl.replace('/api', '');

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notify: NotificationService
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate([getHomeRoute(this.authService.userRole())]);
    }

    this.loginForm = this.fb.group({
      email:      ['', [Validators.required, Validators.email]],
      password:   ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false],
    });
  }

  ngOnInit(): void {
    const ssoError = this.route.snapshot.queryParamMap.get('sso_error');
    if (ssoError) {
      this.notify.error(ssoError);
      this.router.navigate([], { queryParams: {}, replaceUrl: true });
    }
  }

  get email()    { return this.loginForm.get('email')!; }
  get password() { return this.loginForm.get('password')!; }

  onSubmit(): void {
    if (this.loginForm.invalid) { this.loginForm.markAllAsTouched(); return; }

    this.loading.set(true);
    const { email, password } = this.loginForm.value;

    this.authService.login({ email, password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.notify.success(`Welcome back, ${res.data.name}!`);
          this.router.navigate([getHomeRoute(res.data.roleName)]);
        } else {
          this.notify.error(res.message || 'Login failed.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notify.error(err?.error?.message || 'Invalid email or password.');
      },
    });
  }

  signInWith(provider: string): void {
    this.ssoLoading.set(provider);
    // Navigate the browser to the backend SSO initiation endpoint.
    // The backend will redirect to the provider, then back to /auth/sso-callback.
    window.location.href = `${this.apiBase}/api/auth/sso/${provider}`;
  }
}
