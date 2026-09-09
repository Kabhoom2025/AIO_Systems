import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  AccountApiService, AccountDto, CreateAccountDto, UpdateAccountDto
} from '../../../core/account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss'
})
export class AccountsComponent implements OnInit {
  accounts: AccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: AccountDto | null = null;
  saving = false;

  form: { name: string; industry: string | null; website: string | null; phone: string | null; ownerId: number | null } = this.emptyForm();

  constructor(
    private api: AccountApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { name: '', industry: null, website: null, phone: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.accounts = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load accounts.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(account: AccountDto) {
    this.editing = account;
    this.form = { name: account.name, industry: account.industry, website: account.website, phone: account.phone, ownerId: account.ownerId };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || !this.form.ownerId) {
      this.notify.warn('Name and owner are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      industry: this.form.industry,
      website: this.form.website,
      phone: this.form.phone,
      ownerId: this.form.ownerId
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateAccountDto)
      : this.api.create(payload as CreateAccountDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Account ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save account.');
      }
    });
  }

  delete(account: AccountDto) {
    this.confirm.confirm({
      message: `Delete account "${account.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(account.id).subscribe({
          next: () => {
            this.notify.success('Account deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete account.')
        });
      }
    });
  }
}
