import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { OrganizationService } from '../../core/services/organization.service';
import { PlatformModuleService } from '../../core/services/platform-module.service';
import { PharmacyDataService } from '../../core/services/pharmacy-data.service';
import { Organization, License, OrgReport, OrgUser } from '../../core/models/organization.model';
import { OrgModuleStatus } from '../../core/models/platform-module.model';
import { Medicine, ExpiryAlert, Prescription } from '../../core/models/pharmacy.model';

@Component({
  selector: 'app-organization-detail',
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    MatCardModule,
    MatTabsModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
  ],
  templateUrl: './organization-detail.component.html',
  styleUrl: './organization-detail.component.scss',
})
export class OrganizationDetailComponent implements OnInit {
  orgId!: number;
  organization = signal<Organization | null>(null);
  loading = signal(true);

  license = signal<License | null>(null);
  licenseForm = { plan: 'Basic', status: 'Trial', expiryDate: '', maxUsers: 5, notes: '' };

  moduleStatus = signal<OrgModuleStatus | null>(null);
  orgUsers = signal<OrgUser[]>([]);

  hasRestaurantModule = signal(false);
  hasPharmacyModule = signal(false);

  restaurantReport = signal<OrgReport | null>(null);
  restaurantLoading = signal(false);
  restaurantError = signal('');

  pharmacyMedicines = signal<Medicine[]>([]);
  pharmacyExpiryAlerts = signal<ExpiryAlert[]>([]);
  pharmacyPrescriptions = signal<Prescription[]>([]);
  pharmacyLoading = signal(false);
  pharmacyError = signal('');

  medicineColumns = ['name', 'category', 'manufacturer', 'totalStock', 'mrp'];
  prescriptionColumns = ['patientName', 'doctorName', 'prescriptionDate', 'status'];
  userColumns = ['name', 'email', 'roleName', 'isActive'];

  constructor(
    private route: ActivatedRoute,
    private orgService: OrganizationService,
    private moduleService: PlatformModuleService,
    private pharmacyDataService: PharmacyDataService
  ) {}

  ngOnInit(): void {
    this.orgId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.orgService.getById(this.orgId).subscribe({
      next: (org) => {
        this.organization.set(org);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });

    this.orgService.getOrgUsers(this.orgId).subscribe((users) => this.orgUsers.set(users));

    this.orgService.getLicense(this.orgId).pipe(catchError(() => of(null))).subscribe((license) => {
      this.license.set(license);
      if (license) {
        this.licenseForm = {
          plan: license.plan,
          status: license.status,
          expiryDate: license.expiryDate.substring(0, 10),
          maxUsers: license.maxUsers,
          notes: license.notes ?? '',
        };
      }
    });

    this.moduleService.getOrgStatus(this.orgId).subscribe((status) => {
      this.moduleStatus.set(status);
      this.hasRestaurantModule.set(
        status.modules.some((m) => m.moduleKey === 'restaurant' && m.isEnabled)
      );
      this.hasPharmacyModule.set(
        status.modules.some((m) => m.moduleKey === 'pharmacy' && m.isEnabled)
      );
    });
  }

  saveLicense(): void {
    this.orgService
      .upsertLicense(this.orgId, { organizationId: this.orgId, ...this.licenseForm })
      .subscribe((license) => this.license.set(license));
  }

  toggleModule(moduleId: number, enabled: boolean): void {
    const status = this.moduleStatus();
    if (!status) return;
    const currentlyEnabledIds = status.modules.filter((m) => m.isEnabled).map((m) => m.moduleId);
    const nextIds = enabled
      ? [...currentlyEnabledIds, moduleId]
      : currentlyEnabledIds.filter((id) => id !== moduleId);

    this.moduleService
      .assignToOrg({ organizationId: this.orgId, moduleIds: nextIds })
      .subscribe(() => this.load());
  }

  loadRestaurantData(): void {
    if (this.restaurantReport() || this.restaurantLoading()) return;
    this.restaurantLoading.set(true);
    this.restaurantError.set('');
    this.orgService.getRestaurantOrgReport(this.orgId).subscribe({
      next: (report) => {
        this.restaurantReport.set(report);
        this.restaurantLoading.set(false);
      },
      error: () => {
        this.restaurantError.set('Could not reach the Restaurant service for this tenant.');
        this.restaurantLoading.set(false);
      },
    });
  }

  loadPharmacyData(): void {
    if (this.pharmacyMedicines().length || this.pharmacyLoading()) return;
    this.pharmacyLoading.set(true);
    this.pharmacyError.set('');

    this.pharmacyDataService.getMedicines(this.orgId).subscribe({
      next: (meds) => this.pharmacyMedicines.set(meds),
      error: () => this.pharmacyError.set('Could not reach the Pharmacy service for this tenant.'),
    });
    this.pharmacyDataService.getExpiryAlerts(this.orgId).subscribe({
      next: (alerts) => this.pharmacyExpiryAlerts.set(alerts),
      error: () => {},
    });
    this.pharmacyDataService.getPrescriptions(this.orgId).subscribe({
      next: (prescriptions) => {
        this.pharmacyPrescriptions.set(prescriptions);
        this.pharmacyLoading.set(false);
      },
      error: () => this.pharmacyLoading.set(false),
    });
  }
}
