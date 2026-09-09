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
  ContactApiService, ContactDto, CreateContactDto, UpdateContactDto
} from '../../../core/contact-api.service';
import { AccountApiService, AccountDto } from '../../../core/account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-contacts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './contacts.component.html',
  styleUrl: './contacts.component.scss'
})
export class ContactsComponent implements OnInit {
  contacts: ContactDto[] = [];
  accounts: AccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: ContactDto | null = null;
  saving = false;

  form: {
    accountId: number | null; firstName: string; lastName: string;
    email: string | null; phone: string | null; title: string | null; ownerId: number | null;
  } = this.emptyForm();

  constructor(
    private api: ContactApiService,
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
    return { accountId: null, firstName: '', lastName: '', email: null, phone: null, title: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.contacts = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load contacts.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(contact: ContactDto) {
    this.editing = contact;
    this.form = {
      accountId: contact.accountId, firstName: contact.firstName, lastName: contact.lastName,
      email: contact.email, phone: contact.phone, title: contact.title, ownerId: contact.ownerId
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.firstName || !this.form.lastName || !this.form.ownerId) {
      this.notify.warn('First name, last name and owner are required.');
      return;
    }
    this.saving = true;
    const payload = {
      accountId: this.form.accountId,
      firstName: this.form.firstName,
      lastName: this.form.lastName,
      email: this.form.email,
      phone: this.form.phone,
      title: this.form.title,
      ownerId: this.form.ownerId
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateContactDto)
      : this.api.create(payload as CreateContactDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Contact ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save contact.');
      }
    });
  }

  delete(contact: ContactDto) {
    this.confirm.confirm({
      message: `Delete contact "${contact.firstName} ${contact.lastName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(contact.id).subscribe({
          next: () => {
            this.notify.success('Contact deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete contact.')
        });
      }
    });
  }
}
