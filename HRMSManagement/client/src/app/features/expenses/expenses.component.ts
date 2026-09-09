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
import { CalendarModule } from 'primeng/calendar';
import { TabViewModule } from 'primeng/tabview';

import {
  ExpenseApiService,
  ExpenseClaimDto,
  CreateExpenseClaimDto,
  ExpenseSummaryDto
} from '../../core/expense-api.service';
import { NotificationService } from '../../core/notification.service';
import { HasPermissionDirective } from '../../core/permission.directive';

const CATEGORIES = ['Travel', 'Food', 'Accommodation', 'Office Supplies', 'Communication', 'Other'];
const STATUSES = ['Pending', 'Approved', 'Reimbursed', 'Rejected'];

@Component({
  selector: 'app-expenses',
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
    CalendarModule,
    TabViewModule,
    HasPermissionDirective
  ],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss'
})
export class ExpensesComponent implements OnInit {
  categories = CATEGORIES;
  statuses = STATUSES;

  myClaims: ExpenseClaimDto[] = [];
  allClaims: ExpenseClaimDto[] = [];
  summary: ExpenseSummaryDto | null = null;

  loadingMy = false;
  loadingAll = false;

  statusFilter: string | null = null;

  // New claim dialog
  showClaimDialog = false;
  claimForm: Omit<CreateExpenseClaimDto, 'claimDate'> & { claimDate: Date } = this.emptyClaimForm();

  // Review dialog (approve / reject)
  showReviewDialog = false;
  reviewMode: 'approve' | 'reject' = 'approve';
  reviewTarget: ExpenseClaimDto | null = null;
  reviewNotes = '';

  constructor(private expenseApi: ExpenseApiService, private notify: NotificationService) {}

  ngOnInit() {
    this.loadMy();
    this.loadSummary();
    this.loadAll();
  }

  emptyClaimForm(): Omit<CreateExpenseClaimDto, 'claimDate'> & { claimDate: Date } {
    return {
      category: 'Travel',
      claimDate: new Date(),
      amount: 0,
      currency: 'INR',
      description: '',
      receiptUrl: ''
    };
  }

  loadMy() {
    this.loadingMy = true;
    this.expenseApi.my().subscribe({
      next: data => {
        this.myClaims = data;
        this.loadingMy = false;
      },
      error: err => {
        this.loadingMy = false;
        this.notify.error(err.error?.message ?? 'Failed to load your claims.');
      }
    });
  }

  loadAll() {
    this.loadingAll = true;
    this.expenseApi.getAll(this.statusFilter).subscribe({
      next: data => {
        this.allClaims = data;
        this.loadingAll = false;
      },
      error: err => {
        this.loadingAll = false;
        this.notify.error(err.error?.message ?? 'Failed to load claims.');
      }
    });
  }

  loadSummary() {
    this.expenseApi.summary().subscribe({
      next: data => (this.summary = data),
      error: () => {}
    });
  }

  onStatusFilterChange() {
    this.loadAll();
  }

  openNewClaim() {
    this.claimForm = this.emptyClaimForm();
    this.showClaimDialog = true;
  }

  private toDateOnly(d: any): string {
    if (!d) return '';
    const date = d instanceof Date ? d : new Date(d);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  submitClaim() {
    if (!this.claimForm.description.trim() || !this.claimForm.amount) {
      this.notify.warn('Please fill amount and description.');
      return;
    }
    const dto: CreateExpenseClaimDto = {
      ...this.claimForm,
      claimDate: this.toDateOnly(this.claimForm.claimDate)
    };
    this.expenseApi.create(dto).subscribe({
      next: () => {
        this.notify.success('Expense claim submitted.');
        this.showClaimDialog = false;
        this.loadMy();
        this.loadSummary();
        this.loadAll();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to submit claim.')
    });
  }

  deleteClaim(claim: ExpenseClaimDto) {
    if (!confirm('Delete this expense claim?')) return;
    this.expenseApi.delete(claim.id).subscribe({
      next: () => {
        this.notify.success('Claim deleted.');
        this.loadMy();
        this.loadSummary();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete claim.')
    });
  }

  openReview(claim: ExpenseClaimDto, mode: 'approve' | 'reject') {
    this.reviewTarget = claim;
    this.reviewMode = mode;
    this.reviewNotes = '';
    this.showReviewDialog = true;
  }

  submitReview() {
    if (!this.reviewTarget) return;
    const dto = { notes: this.reviewNotes };
    const req$ =
      this.reviewMode === 'approve'
        ? this.expenseApi.approve(this.reviewTarget.id, dto)
        : this.expenseApi.reject(this.reviewTarget.id, dto);
    req$.subscribe({
      next: () => {
        this.notify.success(`Claim ${this.reviewMode === 'approve' ? 'approved' : 'rejected'}.`);
        this.showReviewDialog = false;
        this.loadAll();
        this.loadSummary();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to review claim.')
    });
  }

  markReimbursed(claim: ExpenseClaimDto) {
    this.expenseApi.reimburse(claim.id).subscribe({
      next: () => {
        this.notify.success('Claim marked as reimbursed.');
        this.loadAll();
        this.loadSummary();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark as reimbursed.')
    });
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Pending':
        return 'warn';
      case 'Approved':
        return 'info';
      case 'Reimbursed':
        return 'success';
      case 'Rejected':
        return 'danger';
      default:
        return 'secondary';
    }
  }
}
