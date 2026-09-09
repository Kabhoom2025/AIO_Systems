import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PatientAuthService } from '../../../core/patient-auth.service';

@Component({
  selector: 'app-patient-verify',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './patient-verify.component.html',
  styleUrl: '../patient-login/patient-login.component.scss'
})
export class PatientVerifyComponent implements OnInit {
  phone = '';
  code = '';
  devOtp = '';
  loading = signal(false);
  error = signal('');

  constructor(
    private auth: PatientAuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    this.phone = params.get('phone') ?? '';
    this.devOtp = params.get('devOtp') ?? '';
    if (!this.phone) this.router.navigateByUrl('/patient/login');
  }

  verify() {
    if (!/^\d{6}$/.test(this.code)) {
      this.error.set('Enter the 6-digit OTP.');
      return;
    }
    this.error.set('');
    this.loading.set(true);
    this.auth.verifyOtp(this.phone, this.code).subscribe({
      next: res => {
        this.loading.set(false);
        this.auth.saveSession(res);
        this.router.navigateByUrl('/patient/home');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Invalid or expired OTP. Please try again.');
      }
    });
  }

  resend() {
    this.router.navigate(['/patient/login']);
  }
}
