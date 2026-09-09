import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  LeadApiService, LeadDto, CreateLeadDto, UpdateLeadDto, ConvertLeadDto
} from '../../../core/lead-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

const EDITABLE_STATUSES = ['New', 'Contacted', 'Qualified', 'Lost'];

@Component({
  selector: 'app-leads',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './leads.component.html',
  styleUrl: './leads.component.scss'
})
export class LeadsComponent implements OnInit {
  leads: LeadDto[] = [];
  users: UserDto[] = [];
  statuses = EDITABLE_STATUSES;
  loading = false;

  showDialog = false;
  editing: LeadDto | null = null;
  saving = false;
  form: {
    name: string; companyName: string | null; email: string | null; phone: string | null;
    source: string | null; status: string; ownerId: number | null;
  } = this.emptyForm();

  showConvertDialog = false;
  converting: LeadDto | null = null;
  convertSaving = false;
  convertForm: { accountName: string; createOpportunity: boolean; opportunityName: string; opportunityAmount: number | null } = this.emptyConvertForm();

  constructor(
    private api: LeadApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { name: '', companyName: null, email: null, phone: null, source: null, status: 'New', ownerId: null };
  }

  private emptyConvertForm() {
    return { accountName: '', createOpportunity: false, opportunityName: '', opportunityAmount: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.leads = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leads.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(lead: LeadDto) {
    this.editing = lead;
    this.form = {
      name: lead.name, companyName: lead.companyName, email: lead.email, phone: lead.phone,
      source: lead.source, status: lead.status, ownerId: lead.ownerId
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || !this.form.ownerId) {
      this.notify.warn('Name and owner are required.');
      return;
    }
    this.saving = true;
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          name: this.form.name, companyName: this.form.companyName, email: this.form.email,
          phone: this.form.phone, source: this.form.source, status: this.form.status, ownerId: this.form.ownerId
        } as UpdateLeadDto)
      : this.api.create({
          name: this.form.name, companyName: this.form.companyName, email: this.form.email,
          phone: this.form.phone, source: this.form.source, ownerId: this.form.ownerId
        } as CreateLeadDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Lead ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save lead.');
      }
    });
  }

  delete(lead: LeadDto) {
    this.confirm.confirm({
      message: `Delete lead "${lead.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(lead.id).subscribe({
          next: () => {
            this.notify.success('Lead deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete lead.')
        });
      }
    });
  }

  openConvert(lead: LeadDto) {
    this.converting = lead;
    this.convertForm = this.emptyConvertForm();
    this.convertForm.accountName = lead.companyName ?? lead.name;
    this.showConvertDialog = true;
  }

  convert() {
    if (this.convertForm.createOpportunity && (!this.convertForm.opportunityName || this.convertForm.opportunityAmount == null)) {
      this.notify.warn('Opportunity name and amount are required when creating an opportunity.');
      return;
    }
    this.convertSaving = true;
    const dto: ConvertLeadDto = {
      accountName: this.convertForm.accountName || null,
      createOpportunity: this.convertForm.createOpportunity,
      opportunityName: this.convertForm.createOpportunity ? this.convertForm.opportunityName : null,
      opportunityAmount: this.convertForm.createOpportunity ? this.convertForm.opportunityAmount : null
    };
    this.api.convert(this.converting!.id, dto).subscribe({
      next: () => {
        this.convertSaving = false;
        this.showConvertDialog = false;
        this.notify.success('Lead converted.');
        this.load();
      },
      error: err => {
        this.convertSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to convert lead.');
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Converted') return 'success';
    if (status === 'Lost') return 'danger';
    if (status === 'Qualified') return 'warn';
    return 'info';
  }
}
