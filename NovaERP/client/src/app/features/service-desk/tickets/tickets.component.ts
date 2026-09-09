import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ServiceTicketApiService, ServiceTicketDto, CreateServiceTicketDto, UpdateServiceTicketDto
} from '../../../core/service-ticket-api.service';
import { TicketCategoryApiService, TicketCategoryDto } from '../../../core/ticket-category-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputTextarea, DropdownModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './tickets.component.html',
  styleUrl: './tickets.component.scss'
})
export class TicketsComponent implements OnInit {
  tickets: ServiceTicketDto[] = [];
  categories: TicketCategoryDto[] = [];
  employees: EmployeeDto[] = [];
  loading = false;
  priorities = PRIORITIES;

  showDialog = false;
  editing: ServiceTicketDto | null = null;
  saving = false;

  form: {
    subject: string;
    description: string;
    categoryId: number | null;
    requesterId: number | null;
    priority: string;
  } = this.emptyForm();

  showAssignDialog = false;
  assigning: ServiceTicketDto | null = null;
  assignEmployeeId: number | null = null;

  showResolveDialog = false;
  resolving: ServiceTicketDto | null = null;
  resolutionNotes = '';

  constructor(
    private api: ServiceTicketApiService,
    private categoryApi: TicketCategoryApiService,
    private employeeApi: EmployeeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.categoryApi.getAll().subscribe({ next: rows => (this.categories = rows), error: () => (this.categories = []) });
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
  }

  private emptyForm() {
    return { subject: '', description: '', categoryId: null, requesterId: null, priority: 'Medium' };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.tickets = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load tickets.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(ticket: ServiceTicketDto) {
    this.editing = ticket;
    this.form = {
      subject: ticket.subject, description: ticket.description,
      categoryId: ticket.categoryId, requesterId: ticket.requesterId, priority: ticket.priority
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.subject || !this.form.categoryId || (!this.editing && !this.form.requesterId)) {
      this.notify.warn('Subject, category, and requester are required.');
      return;
    }
    this.saving = true;
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          subject: this.form.subject, description: this.form.description,
          categoryId: this.form.categoryId, priority: this.form.priority
        } as UpdateServiceTicketDto)
      : this.api.create({
          subject: this.form.subject, description: this.form.description,
          categoryId: this.form.categoryId, requesterId: this.form.requesterId,
          priority: this.form.priority
        } as CreateServiceTicketDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Ticket ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save ticket.');
      }
    });
  }

  delete(ticket: ServiceTicketDto) {
    this.confirm.confirm({
      message: `Delete ticket "${ticket.subject}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(ticket.id).subscribe({
          next: () => {
            this.notify.success('Ticket deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete ticket.')
        });
      }
    });
  }

  openAssign(ticket: ServiceTicketDto) {
    this.assigning = ticket;
    this.assignEmployeeId = ticket.assignedToId;
    this.showAssignDialog = true;
  }

  confirmAssign() {
    if (!this.assigning || !this.assignEmployeeId) {
      this.notify.warn('Select an employee.');
      return;
    }
    this.api.assign(this.assigning.id, { employeeId: this.assignEmployeeId }).subscribe({
      next: () => {
        this.showAssignDialog = false;
        this.notify.success('Ticket assigned.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to assign ticket.')
    });
  }

  openResolve(ticket: ServiceTicketDto) {
    this.resolving = ticket;
    this.resolutionNotes = '';
    this.showResolveDialog = true;
  }

  confirmResolve() {
    if (!this.resolving || !this.resolutionNotes) {
      this.notify.warn('Resolution notes are required.');
      return;
    }
    this.api.resolve(this.resolving.id, { resolutionNotes: this.resolutionNotes }).subscribe({
      next: () => {
        this.showResolveDialog = false;
        this.notify.success('Ticket resolved.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to resolve ticket.')
    });
  }

  close(ticket: ServiceTicketDto) {
    this.confirm.confirm({
      message: `Close ticket "${ticket.subject}"?`,
      header: 'Confirm Close',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.close(ticket.id).subscribe({
          next: () => {
            this.notify.success('Ticket closed.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to close ticket.')
        });
      }
    });
  }

  reopen(ticket: ServiceTicketDto) {
    this.api.reopen(ticket.id).subscribe({
      next: () => {
        this.notify.success('Ticket reopened.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to reopen ticket.')
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' | 'secondary' {
    if (status === 'Open') return 'info';
    if (status === 'InProgress') return 'warn';
    if (status === 'Resolved') return 'success';
    return 'secondary';
  }

  prioritySeverity(priority: string): 'success' | 'danger' | 'info' | 'warn' {
    if (priority === 'Low') return 'success';
    if (priority === 'Medium') return 'info';
    if (priority === 'High') return 'warn';
    return 'danger';
  }
}
