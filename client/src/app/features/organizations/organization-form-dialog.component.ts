import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Organization } from '../../core/models/organization.model';

export interface OrganizationFormResult {
  name: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  logoUrl?: string | null;
  timezone?: string | null;
  currency?: string | null;
  isActive: boolean;
}

@Component({
  selector: 'app-organization-form-dialog',
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './organization-form-dialog.component.html',
})
export class OrganizationFormDialogComponent {
  model: OrganizationFormResult;
  isEdit: boolean;

  constructor(
    private ref: MatDialogRef<OrganizationFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { organization?: Organization }
  ) {
    this.isEdit = !!data?.organization;
    const org = data?.organization;
    this.model = {
      name: org?.name ?? '',
      address: org?.address ?? '',
      phone: org?.phone ?? '',
      email: org?.email ?? '',
      logoUrl: org?.logoUrl ?? '',
      timezone: org?.timezone ?? '',
      currency: org?.currency ?? 'INR',
      isActive: org?.isActive ?? true,
    };
  }

  save(): void {
    if (!this.model.name.trim()) return;
    this.ref.close(this.model);
  }

  cancel(): void {
    this.ref.close();
  }
}
