import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { OrganizationService } from '../../../core/services/organization.service';
import { License, UpsertLicenseRequest, Organization } from '../../../core/models/organization.model';

// ── Assign / Edit License Dialog ─────────────────────────────────────────────

export interface LicenseDialogData {
  license?: License;
  organizations: Organization[];
}

@Component({
  selector: 'app-license-dialog',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatSelectModule,
    MatDatepickerModule,
  ],
  template: `
    <div class="ld-wrap">
      <div class="ld-header">
        <mat-icon>{{ data.license ? 'edit' : 'verified' }}</mat-icon>
        <span>{{ data.license ? 'Edit License' : 'Assign License' }}</span>
      </div>
      <form [formGroup]="form" class="ld-body" (ngSubmit)="submit()">

        <mat-form-field appearance="outline">
          <mat-label>Branch / Organization</mat-label>
          <mat-select formControlName="organizationId">
            @for (org of data.organizations; track org.id) {
              <mat-option [value]="org.id">{{ org.name }}</mat-option>
            }
          </mat-select>
          <mat-icon matSuffix>business</mat-icon>
        </mat-form-field>

        <div class="ld-row">
          <mat-form-field appearance="outline">
            <mat-label>Plan</mat-label>
            <mat-select formControlName="plan">
              <mat-option value="Basic">Basic</mat-option>
              <mat-option value="Pro">Pro</mat-option>
              <mat-option value="Enterprise">Enterprise</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option value="Active">Active</mat-option>
              <mat-option value="Trial">Trial</mat-option>
              <mat-option value="Expired">Expired</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="ld-row">
          <!-- Calendar datepicker -->
          <mat-form-field appearance="outline">
            <mat-label>Expiry Date</mat-label>
            <input matInput [matDatepicker]="expiryPicker" formControlName="expiryDate"
                   placeholder="Pick a date" readonly />
            <mat-datepicker-toggle matIconSuffix [for]="expiryPicker"></mat-datepicker-toggle>
            <mat-datepicker #expiryPicker></mat-datepicker>
          </mat-form-field>

          <!-- Time list select -->
          <mat-form-field appearance="outline">
            <mat-label>Expiry Time</mat-label>
            <mat-icon matPrefix class="ld-prefix-icon">schedule</mat-icon>
            <mat-select formControlName="expiryTime">
              @for (t of timeOptions; track t) {
                <mat-option [value]="t">{{ t }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Max Users</mat-label>
          <input matInput type="number" formControlName="maxUsers" min="1" />
          <mat-icon matSuffix>group</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Notes (optional)</mat-label>
          <textarea matInput formControlName="notes" rows="2"></textarea>
        </mat-form-field>

        <div class="ld-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="form.invalid">
            <mat-icon>{{ data.license ? 'save' : 'add' }}</mat-icon>
            {{ data.license ? 'Save Changes' : 'Assign License' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .ld-wrap { width: 480px; font-family: inherit; }
    .ld-header { display: flex; align-items: center; gap: 10px; background: var(--primary, #1a237e); color: #fff; padding: 16px 20px; font-size: 1rem; font-weight: 700; mat-icon { font-size: 22px; } }
    .ld-body { padding: 20px; display: flex; flex-direction: column; gap: 10px; }
    .ld-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    mat-form-field { width: 100%; }
    .ld-prefix-icon { font-size: 18px; width: 18px; height: 18px; color: #94a3b8; margin-right: 4px; }
    .ld-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: 10px; border-top: 1px solid #f0f0f0; }
    .btn-cancel { background: none; border: 1px solid #ddd; border-radius: 8px; padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666; &:hover { background: #f5f5f5; } }
    .btn-save { display: flex; align-items: center; gap: 6px; background: var(--primary, #1a237e); color: #fff; border: none; border-radius: 8px; padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } mat-icon { font-size: 18px; } }
  `],
})
export class LicenseDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<LicenseDialogComponent>);
  readonly data = inject<LicenseDialogData>(MAT_DIALOG_DATA);

  form: FormGroup;

  // 30-minute interval time list: 00:00 … 23:30
  readonly timeOptions: string[] = Array.from({ length: 48 }, (_, i) => {
    const h = Math.floor(i / 2).toString().padStart(2, '0');
    const m = i % 2 === 0 ? '00' : '30';
    return `${h}:${m}`;
  });

  constructor() {
    const l = this.data.license;
    let dateVal: Date | null = null;
    let timeVal = '23:30';

    if (l) {
      const d = new Date(l.expiryDate);
      dateVal = d;
      // Snap to nearest 30-min slot
      const snappedM = d.getMinutes() < 30 ? '00' : '30';
      timeVal = `${d.getHours().toString().padStart(2, '0')}:${snappedM}`;
    }

    this.form = this.fb.group({
      organizationId: [{ value: l?.organizationId ?? null, disabled: !!l }, Validators.required],
      plan:           [l?.plan ?? 'Basic', Validators.required],
      status:         [l?.status ?? 'Trial', Validators.required],
      expiryDate:     [dateVal, Validators.required],
      expiryTime:     [timeVal, Validators.required],
      maxUsers:       [l?.maxUsers ?? 5, [Validators.required, Validators.min(1)]],
      notes:          [l?.notes ?? ''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const raw = this.form.getRawValue();
    const d = new Date(raw.expiryDate as Date);
    const [hh, mm] = (raw.expiryTime as string).split(':');
    d.setHours(parseInt(hh, 10), parseInt(mm, 10), 0, 0);
    const result: UpsertLicenseRequest = {
      organizationId: raw.organizationId,
      plan: raw.plan,
      status: raw.status,
      expiryDate: d.toISOString(),
      maxUsers: +raw.maxUsers,
      notes: raw.notes || undefined,
    };
    this.dialogRef.close(result);
  }

  cancel(): void { this.dialogRef.close(); }
}

// ── Main Component ────────────────────────────────────────────────────────────

@Component({
  selector: 'app-sa-licenses',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatProgressSpinnerModule,
    MatDialogModule, MatTooltipModule,
  ],
  templateUrl: './sa-licenses.component.html',
  styleUrl: './sa-licenses.component.scss',
})
export class SaLicensesComponent implements OnInit {
  private orgSvc  = inject(OrganizationService);
  private dialog  = inject(MatDialog);
  private snack   = inject(MatSnackBar);

  licenses      = signal<License[]>([]);
  organizations = signal<Organization[]>([]);
  loading       = signal(false);
  searchTerm    = signal('');

  filtered = computed(() => {
    const q = this.searchTerm().toLowerCase();
    return q
      ? this.licenses().filter(l => l.orgName.toLowerCase().includes(q) || l.plan.toLowerCase().includes(q))
      : this.licenses();
  });

  stats = computed(() => ({
    total:   this.licenses().length,
    active:  this.licenses().filter(l => l.status === 'Active').length,
    trial:   this.licenses().filter(l => l.status === 'Trial').length,
    expired: this.licenses().filter(l => l.status === 'Expired').length,
  }));

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.orgSvc.getAllLicenses().subscribe({
      next: list => { this.licenses.set(list); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load licenses', '', { duration: 3000, panelClass: 'snack-error' }); },
    });
    this.orgSvc.getAll().subscribe({
      next: orgs => this.organizations.set(orgs),
    });
  }

  openAssign(): void {
    const ref = this.dialog.open(LicenseDialogComponent, {
      data: { organizations: this.organizations() } as LicenseDialogData,
      disableClose: true,
    });
    ref.afterClosed().subscribe((result: UpsertLicenseRequest | undefined) => {
      if (!result) return;
      this.orgSvc.upsertOrgLicense(result.organizationId, result).subscribe({
        next: lic => {
          this.licenses.update(list => {
            const idx = list.findIndex(l => l.organizationId === lic.organizationId);
            return idx >= 0 ? list.map((l, i) => i === idx ? lic : l) : [...list, lic];
          });
          this.snack.open('License assigned.', '', { duration: 2500, panelClass: 'snack-success' });
        },
        error: () => this.snack.open('Failed to assign license.', '', { duration: 3000, panelClass: 'snack-error' }),
      });
    });
  }

  openEdit(license: License): void {
    const ref = this.dialog.open(LicenseDialogComponent, {
      data: { license, organizations: this.organizations() } as LicenseDialogData,
      disableClose: true,
    });
    ref.afterClosed().subscribe((result: UpsertLicenseRequest | undefined) => {
      if (!result) return;
      this.orgSvc.upsertOrgLicense(license.organizationId, result).subscribe({
        next: updated => {
          this.licenses.update(list => list.map(l => l.id === updated.id ? updated : l));
          this.snack.open('License updated.', '', { duration: 2500, panelClass: 'snack-success' });
        },
        error: () => this.snack.open('Failed to update license.', '', { duration: 3000, panelClass: 'snack-error' }),
      });
    });
  }

  confirmDelete(license: License): void {
    if (!confirm(`Remove license for "${license.orgName}"?`)) return;
    this.orgSvc.deleteOrgLicense(license.organizationId).subscribe({
      next: () => {
        this.licenses.update(list => list.filter(l => l.id !== license.id));
        this.snack.open('License removed.', '', { duration: 2500, panelClass: 'snack-success' });
      },
      error: () => this.snack.open('Failed to remove license.', '', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  daysUntilExpiry(expiryDate: string): number {
    const diff = new Date(expiryDate).getTime() - Date.now();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  isExpiringSoon(expiryDate: string): boolean {
    const days = this.daysUntilExpiry(expiryDate);
    return days >= 0 && days <= 30;
  }
}
