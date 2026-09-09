import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PatientAuthService } from '../../../core/patient-auth.service';

@Component({
  selector: 'app-patient-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './patient-login.component.html',
  styleUrl: './patient-login.component.scss'
})
export class PatientLoginComponent {
  phone = '';
  loading = signal(false);
  error = signal('');

  constructor(private auth: PatientAuthService, private router: Router) {}

  sendOtp() {
    const trimmed = this.phone.trim();
    if (!/^\d{10}$/.test(trimmed)) {
      this.error.set('Enter a valid 10-digit mobile number.');
      return;
    }
    this.error.set('');
    this.loading.set(true);
    this.auth.requestOtp(trimmed).subscribe({
      next: res => {
        this.loading.set(false);
        this.router.navigate(['/patient/verify'], {
          queryParams: { phone: trimmed, devOtp: res.devOtp ?? '' }
        });
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Could not send OTP. Please try again.');
      }
    });
  }
}
