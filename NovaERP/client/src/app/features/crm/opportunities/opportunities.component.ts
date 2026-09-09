import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  OpportunityApiService, OpportunityDto, CreateOpportunityDto, UpdateOpportunityDto
} from '../../../core/opportunity-api.service';
import { AccountApiService, AccountDto } from '../../../core/account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

const STAGES = ['Qualification', 'Proposal', 'Negotiation', 'Won', 'Lost'];

@Component({
  selector: 'app-opportunities',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './opportunities.component.html',
  styleUrl: './opportunities.component.scss'
})
export class OpportunitiesComponent implements OnInit {
  opportunities: OpportunityDto[] = [];
  accounts: AccountDto[] = [];
  users: UserDto[] = [];
  stages = STAGES;
  loading = false;

  showDialog = false;
  editing: OpportunityDto | null = null;
  saving = false;

  form: {
    accountId: number | null; name: string; amount: number | null;
    stage: string; closeDate: Date | null; ownerId: number | null;
  } = this.emptyForm();

  constructor(
    private api: OpportunityApiService,
    private accountApi: AccountApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.accountApi.getAll().subscribe({ next: rows => (this.accounts = rows), error: () => (this.accounts = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { accountId: null, name: '', amount: null, stage: 'Qualification', closeDate: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.opportunities = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load opportunities.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(opportunity: OpportunityDto) {
    this.editing = opportunity;
    this.form = {
      accountId: opportunity.accountId, name: opportunity.name, amount: opportunity.amount,
      stage: opportunity.stage, closeDate: opportunity.closeDate ? new Date(opportunity.closeDate) : null,
      ownerId: opportunity.ownerId
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || this.form.amount == null || !this.form.ownerId || (!this.editing && !this.form.accountId)) {
      this.notify.warn('Account, name, amount and owner are required.');
      return;
    }
    this.saving = true;
    const closeDate = this.form.closeDate ? this.form.closeDate.toISOString() : null;
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          name: this.form.name,
          amount: this.form.amount,
          stage: this.form.stage,
          closeDate,
          ownerId: this.form.ownerId
        } as UpdateOpportunityDto)
      : this.api.create({
          accountId: this.form.accountId,
          name: this.form.name,
          amount: this.form.amount,
          stage: this.form.stage,
          closeDate,
          ownerId: this.form.ownerId
        } as CreateOpportunityDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Opportunity ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save opportunity.');
      }
    });
  }

  delete(opportunity: OpportunityDto) {
    this.confirm.confirm({
      message: `Delete opportunity "${opportunity.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(opportunity.id).subscribe({
          next: () => {
            this.notify.success('Opportunity deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete opportunity.')
        });
      }
    });
  }

  stageSeverity(stage: string): 'success' | 'danger' | 'info' | 'warn' {
    if (stage === 'Won') return 'success';
    if (stage === 'Lost') return 'danger';
    if (stage === 'Negotiation') return 'warn';
    return 'info';
  }
}
