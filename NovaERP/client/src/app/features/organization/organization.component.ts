import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { OrganizationApiService, OrganizationDto, UpdateOrganizationDto } from '../../core/organization-api.service';

@Component({
  selector: 'app-organization',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, InputTextarea, ButtonModule, ToastModule, HasPermissionDirective],
  templateUrl: './organization.component.html',
  styleUrl: './organization.component.scss'
})
export class OrganizationComponent implements OnInit {
  profile: OrganizationDto | null = null;
  form: UpdateOrganizationDto = this.emptyForm();
  loading = false;
  saving = false;

  constructor(private api: OrganizationApiService, private notify: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm(): UpdateOrganizationDto {
    return {
      name: '',
      legalName: null,
      address: null,
      phone: null,
      email: null,
      website: null,
      taxNumber: null,
      timezone: 'Asia/Kolkata',
      currency: 'INR',
      logoUrl: null
    };
  }

  load() {
    this.loading = true;
    this.api.getProfile().subscribe({
      next: p => {
        this.profile = p;
        this.form = {
          name: p.name,
          legalName: p.legalName,
          address: p.address,
          phone: p.phone,
          email: p.email,
          website: p.website,
          taxNumber: p.taxNumber,
          timezone: p.timezone,
          currency: p.currency,
          logoUrl: p.logoUrl
        };
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load organization profile.');
      }
    });
  }

  save() {
    if (!this.form.name.trim()) {
      this.notify.warn('Organization name is required.');
      return;
    }
    this.saving = true;
    this.api.updateProfile(this.form).subscribe({
      next: p => {
        this.saving = false;
        this.profile = p;
        this.notify.success('Organization profile updated.');
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update organization profile.');
      }
    });
  }
}
