import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { OrganizationService } from '../../../core/services/organization.service';
import { Organization } from '../../../core/models/organization.model';
import { Settings } from '../../../core/models/settings.model';

@Component({
  selector: 'app-sa-branches',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatIconModule, MatButtonModule, MatProgressSpinnerModule,
    MatTooltipModule, MatDialogModule, MatSlideToggleModule,
    MatFormFieldModule, MatInputModule,
  ],
  templateUrl: './sa-branches.component.html',
  styleUrl: './sa-branches.component.scss',
})
export class SaBranchesComponent implements OnInit {
  private orgSvc  = inject(OrganizationService);
  private snack   = inject(MatSnackBar);

  branches      = signal<Organization[]>([]);
  loading       = signal(false);
  searchTerm    = signal('');

  // Per-branch settings state
  expandedId    = signal<number | null>(null);
  settingsMap   = signal<Map<number, Settings>>(new Map());
  settingsLoading = signal<Map<number, boolean>>(new Map());
  settingsSaving  = signal<Map<number, boolean>>(new Map());
  editForms       = signal<Map<number, FormGroup>>(new Map());

  filtered = computed(() => {
    const q = this.searchTerm().toLowerCase();
    return q
      ? this.branches().filter(b => b.name.toLowerCase().includes(q) || b.address?.toLowerCase().includes(q))
      : this.branches();
  });

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.orgSvc.getAll().subscribe({
      next: list => { this.branches.set(list); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load branches', '', { duration: 3000, panelClass: 'snack-error' }); },
    });
  }

  private orgById(id: number): Organization | undefined {
    return this.branches().find(b => b.id === id);
  }

  toggle(id: number): void {
    if (this.expandedId() === id) {
      this.expandedId.set(null);
      return;
    }
    this.expandedId.set(id);
    if (!this.settingsMap().has(id)) {
      this.loadSettings(id);
    }
  }

  loadSettings(orgId: number): void {
    const org = this.orgById(orgId);
    this.settingsLoading.update(m => new Map(m).set(orgId, true));
    this.orgSvc.getOrgSettings(orgId).subscribe({
      next: s => {
        this.settingsMap.update(m => new Map(m).set(orgId, s));
        this.settingsLoading.update(m => new Map(m).set(orgId, false));
        this.buildForm(orgId, s, org);
      },
      error: () => {
        this.settingsLoading.update(m => new Map(m).set(orgId, false));
        // Build form with org data as fallback when settings unavailable
        this.buildForm(orgId, null, org);
      },
    });
  }

  buildForm(orgId: number, s: Settings | null, org?: Organization): void {
    const form = this.fb.group({
      // Branch's own name/address/phone take priority over global settings fallback
      restaurantName: [org?.name ?? s?.restaurantName ?? '', Validators.required],
      address:        [org?.address ?? s?.address ?? ''],
      phone:          [org?.phone ?? s?.phone ?? ''],
      gstNumber:      [s?.gstNumber ?? ''],
      taxPercentage:  [s?.taxPercentage ?? 0, [Validators.required, Validators.min(0), Validators.max(100)]],
      taxName:        [s?.taxName ?? 'GST'],
      taxInclusive:   [s?.taxInclusive ?? false],
      upiId:          [s?.upiId ?? ''],
    });
    this.editForms.update(m => new Map(m).set(orgId, form));
  }

  getForm(orgId: number): FormGroup | null {
    return this.editForms().get(orgId) ?? null;
  }

  isLoading(orgId: number): boolean { return this.settingsLoading().get(orgId) ?? false; }
  isSaving(orgId: number):  boolean { return this.settingsSaving().get(orgId) ?? false;  }

  saveSettings(org: Organization): void {
    const form = this.getForm(org.id);
    if (!form || form.invalid) { form?.markAllAsTouched(); return; }
    this.settingsSaving.update(m => new Map(m).set(org.id, true));
    this.orgSvc.updateOrgSettings(org.id, form.value).subscribe({
      next: updated => {
        this.settingsMap.update(m => new Map(m).set(org.id, updated));
        this.settingsSaving.update(m => new Map(m).set(org.id, false));
        this.snack.open(`Settings for "${org.name}" saved.`, '', { duration: 2500, panelClass: 'snack-success' });
      },
      error: err => {
        this.settingsSaving.update(m => new Map(m).set(org.id, false));
        this.snack.open(err?.error?.message || 'Failed to save settings.', '', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  orgInitial(name: string): string { return name.charAt(0).toUpperCase(); }
}
