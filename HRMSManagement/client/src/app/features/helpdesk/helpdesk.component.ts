import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';

import {
  HelpDeskApiService,
  TicketDto,
  TicketDetailDto,
  CreateTicketDto,
  HelpDeskSummaryDto
} from '../../core/helpdesk-api.service';
import { NotificationService } from '../../core/notification.service';
import { HasPermissionDirective } from '../../core/permission.directive';

const CATEGORIES = ['HR', 'IT', 'Finance', 'Admin'];
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const STATUSES = ['Open', 'InProgress', 'Resolved', 'Closed'];

@Component({
  selector: 'app-helpdesk',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    InputTextarea,
    DropdownModule,
    TagModule,
    TabViewModule,
    HasPermissionDirective
  ],
  templateUrl: './helpdesk.component.html',
  styleUrl: './helpdesk.component.scss'
})
export class HelpdeskComponent implements OnInit {
  categories = CATEGORIES;
  priorities = PRIORITIES;
  statuses = STATUSES;

  myTickets: TicketDto[] = [];
  allTickets: TicketDto[] = [];
  summary: HelpDeskSummaryDto | null = null;

  loadingMy = false;
  loadingAll = false;

  statusFilter: string | null = null;
  categoryFilter: string | null = null;

  // New ticket dialog
  showTicketDialog = false;
  ticketForm: CreateTicketDto = this.emptyTicketForm();

  // Detail / comments dialog
  showDetailDialog = false;
  detailTicket: TicketDetailDto | null = null;
  newComment = '';

  // Assign dialog
  showAssignDialog = false;
  assignTicket: TicketDto | null = null;
  assignUserId: number | null = null;

  // Status change dialog
  showStatusDialog = false;
  statusTicket: TicketDto | null = null;
  newStatus = 'Open';

  constructor(private helpDeskApi: HelpDeskApiService, private notify: NotificationService) {}

  ngOnInit() {
    this.loadMy();
    this.loadSummary();
    this.loadAll();
  }

  emptyTicketForm(): CreateTicketDto {
    return { category: 'HR', priority: 'Medium', subject: '', description: '' };
  }

  loadMy() {
    this.loadingMy = true;
    this.helpDeskApi.myTickets().subscribe({
      next: data => {
        this.myTickets = data;
        this.loadingMy = false;
      },
      error: err => {
        this.loadingMy = false;
        this.notify.error(err.error?.message ?? 'Failed to load your tickets.');
      }
    });
  }

  loadAll() {
    this.loadingAll = true;
    this.helpDeskApi.getAll(this.statusFilter, this.categoryFilter).subscribe({
      next: data => {
        this.allTickets = data;
        this.loadingAll = false;
      },
      error: err => {
        this.loadingAll = false;
        this.notify.error(err.error?.message ?? 'Failed to load tickets.');
      }
    });
  }

  loadSummary() {
    this.helpDeskApi.summary().subscribe({
      next: data => (this.summary = data),
      error: () => {}
    });
  }

  onFilterChange() {
    this.loadAll();
  }

  openNewTicket() {
    this.ticketForm = this.emptyTicketForm();
    this.showTicketDialog = true;
  }

  submitTicket() {
    if (!this.ticketForm.subject.trim() || !this.ticketForm.description.trim()) {
      this.notify.warn('Please fill subject and description.');
      return;
    }
    this.helpDeskApi.createTicket(this.ticketForm).subscribe({
      next: () => {
        this.notify.success('Ticket created.');
        this.showTicketDialog = false;
        this.loadMy();
        this.loadSummary();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to create ticket.')
    });
  }

  openDetail(ticket: TicketDto) {
    this.helpDeskApi.getById(ticket.id).subscribe({
      next: data => {
        this.detailTicket = data;
        this.newComment = '';
        this.showDetailDialog = true;
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to load ticket details.')
    });
  }

  submitComment() {
    if (!this.detailTicket || !this.newComment.trim()) return;
    this.helpDeskApi.addComment(this.detailTicket.id, { comment: this.newComment }).subscribe({
      next: data => {
        this.detailTicket = data;
        this.newComment = '';
        this.notify.success('Comment added.');
        this.loadMy();
        this.loadAll();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add comment.')
    });
  }

  openAssign(ticket: TicketDto) {
    this.assignTicket = ticket;
    this.assignUserId = ticket.assignedToUserId;
    this.showAssignDialog = true;
  }

  submitAssign() {
    if (!this.assignTicket || !this.assignUserId) {
      this.notify.warn('Please enter a user ID.');
      return;
    }
    this.helpDeskApi.assign(this.assignTicket.id, { userId: this.assignUserId }).subscribe({
      next: () => {
        this.notify.success('Ticket assigned.');
        this.showAssignDialog = false;
        this.loadAll();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to assign ticket.')
    });
  }

  openStatusChange(ticket: TicketDto) {
    this.statusTicket = ticket;
    this.newStatus = ticket.status;
    this.showStatusDialog = true;
  }

  submitStatusChange() {
    if (!this.statusTicket) return;
    this.helpDeskApi.updateStatus(this.statusTicket.id, { status: this.newStatus }).subscribe({
      next: () => {
        this.notify.success('Ticket status updated.');
        this.showStatusDialog = false;
        this.loadAll();
        this.loadSummary();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to update ticket status.')
    });
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Open':
        return 'warn';
      case 'InProgress':
        return 'info';
      case 'Resolved':
        return 'success';
      case 'Closed':
        return 'secondary';
      default:
        return 'secondary';
    }
  }

  prioritySeverity(priority: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (priority) {
      case 'Low':
        return 'success';
      case 'Medium':
        return 'info';
      case 'High':
        return 'warn';
      case 'Critical':
        return 'danger';
      default:
        return 'secondary';
    }
  }
}
