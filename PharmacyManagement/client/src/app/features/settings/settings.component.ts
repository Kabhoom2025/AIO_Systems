import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getAuthHeaders } from '../../core/auth.helper';
import { PermissionsService } from '../../core/permissions.service';
import { NotificationService } from '../../core/notification.service';

interface Organization {
  id: number; name: string; address?: string; phone?: string; email?: string;
  licenseNo?: string; currency: string; gstNumber?: string; invoiceNumberPrefix: string;
}
interface StoredUser {
  name: string; email: string; role: string; organizationName: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  orgId = getOrgIdFromToken();
  activeTab = signal<'organization' | 'profile'>('organization');

  canViewOrg = false;
  canEditOrg = false;

  org = signal<Organization | null>(null);
  form = { name: '', address: '', phone: '', email: '', licenseNo: '', currency: 'INR', gstNumber: '', invoiceNumberPrefix: 'INV' };

  user: StoredUser | null = this.readStoredUser();

  passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
  changingPassword = signal(false);

  constructor(
    private http: HttpClient,
    private permissions: PermissionsService,
    private notify: NotificationService
  ) {
    this.canViewOrg = this.permissions.has('settings.view');
    this.canEditOrg = this.permissions.has('settings.edit');
    if (!this.canViewOrg) this.activeTab.set('profile');
  }

  ngOnInit() {
    if (this.canViewOrg) this.loadOrg();
  }

  private readStoredUser(): StoredUser | null {
    const raw = localStorage.getItem(environment.userKey);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  }

  loadOrg() {
    this.http.get<Organization>(`${environment.apiUrl}/organizations/${this.orgId}`, {
      headers: getAuthHeaders()
    }).subscribe(data => {
      this.org.set(data);
      this.form = {
        name: data.name, address: data.address ?? '', phone: data.phone ?? '',
        email: data.email ?? '', licenseNo: data.licenseNo ?? '', currency: data.currency,
        gstNumber: data.gstNumber ?? '', invoiceNumberPrefix: data.invoiceNumberPrefix
      };
    });
  }

  saveOrg() {
    this.http.put(`${environment.apiUrl}/organizations/${this.orgId}`, this.form, {
      headers: getAuthHeaders()
    }).subscribe({
      next: () => { this.notify.success('Organization settings saved.'); this.loadOrg(); },
      error: () => this.notify.error('Failed to save organization settings.')
    });
  }

  changePassword() {
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.notify.error('New password and confirmation do not match.');
      return;
    }
    this.changingPassword.set(true);
    this.http.post(`${environment.apiUrl}/auth/change-password`, {
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword
    }, { headers: getAuthHeaders() }).subscribe({
      next: () => {
        this.notify.success('Password changed successfully.');
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
        this.changingPassword.set(false);
      },
      error: (err) => {
        this.notify.error(err?.error?.message ?? 'Failed to change password.');
        this.changingPassword.set(false);
      }
    });
  }
}
