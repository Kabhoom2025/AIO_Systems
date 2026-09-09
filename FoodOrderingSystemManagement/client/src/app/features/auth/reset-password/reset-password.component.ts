import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const pw  = group.get('newPassword')?.value;
  const cpw = group.get('confirmPassword')?.value;
  return pw && cpw && pw !== cpw ? { mismatch: true } : null;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './reset-password.component.html',
  styleUrl:    './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
  private fb    = inject(FormBuilder);
  private http  = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  token     = signal('');
  loading   = signal(false);
  success   = signal(false);
  error     = signal<string | null>(null);
  hideNew   = signal(true);
  hideConf  = signal(true);

  form = this.fb.group({
    newPassword:     ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordsMatch });

  get newPassword()     { return this.form.get('newPassword')!; }
  get confirmPassword() { return this.form.get('confirmPassword')!; }

  ngOnInit(): void {
    const t = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!t) {
      this.error.set('Reset link is missing or invalid. Please request a new one.');
    }
    this.token.set(t);
  }

  submit(): void {
    if (this.form.invalid || !this.token()) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);

    this.http.post<ApiResponse<string>>(
      `${environment.apiUrl}/auth/reset-password`,
      {
        token:           this.token(),
        newPassword:     this.newPassword.value,
        confirmPassword: this.confirmPassword.value,
      }
    ).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set(true);
        setTimeout(() => this.router.navigate(['/auth/login']), 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Failed to reset password. The link may have expired.');
      },
    });
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
