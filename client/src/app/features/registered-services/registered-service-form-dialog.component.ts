import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { RegisteredService } from '../../core/models/registered-service.model';

export interface RegisteredServiceFormResult {
  name: string;
  routePrefix: string;
  baseUrl: string;
  description?: string | null;
  moduleKeys: string;
  healthCheckPath?: string | null;
  isActive: boolean;

  backendWorkingDirectory?: string | null;
  backendCommand?: string | null;
  frontendUrl?: string | null;
  frontendWorkingDirectory?: string | null;
  frontendCommand?: string | null;
  dockerServiceName?: string | null;

  icon: string;
  color: string;
  sortOrder: number;
}

@Component({
  selector: 'app-registered-service-form-dialog',
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './registered-service-form-dialog.component.html',
})
export class RegisteredServiceFormDialogComponent {
  model: RegisteredServiceFormResult;
  isEdit: boolean;

  constructor(
    private ref: MatDialogRef<RegisteredServiceFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { service?: RegisteredService }
  ) {
    this.isEdit = !!data?.service;
    const s = data?.service;
    this.model = {
      name: s?.name ?? '',
      routePrefix: s?.routePrefix ?? '',
      baseUrl: s?.baseUrl ?? '',
      description: s?.description ?? '',
      moduleKeys: s?.moduleKeys ?? '',
      healthCheckPath: s?.healthCheckPath ?? '',
      isActive: s?.isActive ?? true,
      backendWorkingDirectory: s?.backendWorkingDirectory ?? '',
      backendCommand: s?.backendCommand ?? '',
      frontendUrl: s?.frontendUrl ?? '',
      frontendWorkingDirectory: s?.frontendWorkingDirectory ?? '',
      frontendCommand: s?.frontendCommand ?? '',
      dockerServiceName: s?.dockerServiceName ?? '',
      icon: s?.icon ?? 'dns',
      color: s?.color ?? '#6c757d',
      sortOrder: s?.sortOrder ?? 0,
    };
  }

  save(): void {
    if (!this.model.name.trim() || !this.model.routePrefix.trim() || !this.model.baseUrl.trim()) return;
    this.ref.close(this.model);
  }

  cancel(): void {
    this.ref.close();
  }
}
