import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'app-sa-api-config',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule, FormsModule,
    MatIconModule, MatButtonModule, MatProgressSpinnerModule, MatSlideToggleModule,
  ],
  templateUrl: './sa-api-config.component.html',
  styleUrl: './sa-api-config.component.scss',
})
export class SaApiConfigComponent {
  private fb = inject(FormBuilder);

  configForm = this.fb.group({
    rateLimit:       [120, [Validators.required, Validators.min(1)]],
    corsOrigins:     ['https://app.example.com\nhttps://admin.example.com'],
    jwtExpiryHours:  [24, [Validators.required, Validators.min(1)]],
    maxUploadMb:     [10, [Validators.required, Validators.min(1)]],
    debugMode:       [false],
    maintenanceMode: [false],
  });

  onSave(): void {
    if (this.configForm.valid) {
      console.log('Config saved (mock):', this.configForm.value);
    }
  }
}
