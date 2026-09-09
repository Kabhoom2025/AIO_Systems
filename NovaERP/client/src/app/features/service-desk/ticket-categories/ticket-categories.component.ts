import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  TicketCategoryApiService, TicketCategoryDto, CreateTicketCategoryDto, UpdateTicketCategoryDto
} from '../../../core/ticket-category-api.service';

@Component({
  selector: 'app-ticket-categories',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, CheckboxModule, TagModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './ticket-categories.component.html',
  styleUrl: './ticket-categories.component.scss'
})
export class TicketCategoriesComponent implements OnInit {
  categories: TicketCategoryDto[] = [];
  loading = false;

  showDialog = false;
  editing: TicketCategoryDto | null = null;
  saving = false;

  form: { name: string; code: string; isActive: boolean } = this.emptyForm();

  constructor(
    private api: TicketCategoryApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { name: '', code: '', isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.categories = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load ticket categories.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(category: TicketCategoryDto) {
    this.editing = category;
    this.form = { name: category.name, code: category.code, isActive: category.isActive };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.saving = true;
    const payload = { name: this.form.name, isActive: this.form.isActive };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateTicketCategoryDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateTicketCategoryDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Ticket category ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save ticket category.');
      }
    });
  }

  delete(category: TicketCategoryDto) {
    this.confirm.confirm({
      message: `Delete ticket category "${category.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(category.id).subscribe({
          next: () => {
            this.notify.success('Ticket category deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete ticket category.')
        });
      }
    });
  }
}
