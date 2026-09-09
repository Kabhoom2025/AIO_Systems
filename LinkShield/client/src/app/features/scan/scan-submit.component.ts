import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { ScanApiService } from '../../core/services/scan-api.service';

@Component({
  selector: 'app-scan-submit',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './scan-submit.component.html',
  styleUrl: './scan-submit.component.scss'
})
export class ScanSubmitComponent {
  private readonly fb = inject(FormBuilder);
  private readonly scanApi = inject(ScanApiService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    url: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/i)]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.scanApi.submitScan(this.form.getRawValue().url).subscribe({
      next: (scan) => this.router.navigate(['/scan', scan.scanId]),
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not submit the URL for scanning.');
      }
    });
  }
}
