import { Component, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { environment } from '../../../environments/environment';
import { PermissionsService } from '../../core/permissions.service';
import { NotificationService } from '../../core/notification.service';

interface LoginResponse {
  token: string; userId: number; name: string; email: string;
  role: string; organizationId: number; organizationName: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ButtonModule, InputTextModule, PasswordModule, ToastModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = 'admin@pharmacy.local';
  password = 'Admin@123';
  loading = signal(false);

  constructor(
    private http: HttpClient,
    private router: Router,
    private permissions: PermissionsService,
    private notify: NotificationService
  ) {}

  login() {
    this.loading.set(true);
    this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, {
      email: this.email, password: this.password
    }).subscribe({
      next: res => {
        localStorage.setItem(environment.tokenKey, res.token);
        localStorage.setItem(environment.userKey, JSON.stringify(res));
        this.permissions.reload();
        this.loading.set(false);
        this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Invalid email or password.');
      }
    });
  }
}
