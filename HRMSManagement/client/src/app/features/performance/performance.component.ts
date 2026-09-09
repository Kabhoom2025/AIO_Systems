import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { RatingModule } from 'primeng/rating';
import { ProgressBarModule } from 'primeng/progressbar';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  PerformanceApiService,
  PerformanceGoalDto,
  CreatePerformanceGoalDto,
  UpdatePerformanceGoalDto,
  UpdateGoalProgressDto,
  PerformanceReviewDto,
  CreatePerformanceReviewDto,
  SelfReviewDto,
  ManagerReviewDto
} from '../../core/performance-api.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface EmployeeLookupDto { id: number; employeeCode: string; fullName: string; designationTitle: string; }

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    TabViewModule, TableModule, DialogModule,
    ButtonModule, InputTextModule, InputTextarea,
    InputNumberModule, DropdownModule, TagModule,
    CalendarModule, RatingModule, ProgressBarModule,
    TooltipModule, ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './performance.component.html',
  styleUrl: './performance.component.scss'
})
export class PerformanceComponent implements OnInit {

  // Lookups
  employees: EmployeeLookupDto[] = [];

  goalStatusOptions = [
    { label: 'Not Started', value: 'NotStarted' },
    { label: 'In Progress', value: 'InProgress' },
    { label: 'Completed',   value: 'Completed'  },
    { label: 'Cancelled',   value: 'Cancelled'  }
  ];

  reviewStatusOptions = [
    { label: 'Draft',     value: 'Draft'     },
    { label: 'Self Done', value: 'SelfDone'  },
    { label: 'Completed', value: 'Completed' }
  ];

  recommendationOptions = [
    { label: 'Promote',      value: 'Promote'      },
    { label: 'Retain',       value: 'Retain'       },
    { label: 'PIP',          value: 'PIP'          },
    { label: 'Terminate',    value: 'Terminate'    },
    { label: 'No Action',    value: 'NoAction'     }
  ];

  // ---- Goals ----
  goals: PerformanceGoalDto[] = [];
  myGoals: PerformanceGoalDto[] = [];
  goalsLoading = false;
  myGoalsLoading = false;

  showGoalDialog = false;
  editingGoalId: number | null = null;
  savingGoal = false;
  goalForm: UpdatePerformanceGoalDto & { employeeId?: number } = this.emptyGoalForm();

  showProgressDialog = false;
  progressTarget: PerformanceGoalDto | null = null;
  progressForm: UpdateGoalProgressDto = { progressPercent: 0, status: null };
  savingProgress = false;

  // ---- Reviews ----
  reviews: PerformanceReviewDto[] = [];
  myReviews: PerformanceReviewDto[] = [];
  reviewsLoading = false;
  myReviewsLoading = false;
  reviewPeriodFilter: string | null = null;

  showReviewDialog = false;
  savingReview = false;
  reviewForm: CreatePerformanceReviewDto = this.emptyReviewForm();

  showSelfReviewDialog = false;
  selfReviewTarget: PerformanceReviewDto | null = null;
  selfReviewForm: SelfReviewDto = { selfRating: 0, selfComments: '' };
  savingSelf = false;

  showManagerReviewDialog = false;
  managerReviewTarget: PerformanceReviewDto | null = null;
  managerReviewForm: ManagerReviewDto = { managerRating: 0, managerComments: '', recommendation: 'Retain' };
  savingManager = false;

  constructor(
    private api: PerformanceApiService,
    private http: HttpClient,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadEmployees();
    this.loadGoals();
    this.loadMyGoals();
    this.loadReviews();
    this.loadMyReviews();
  }

  loadEmployees() {
    this.http.get<EmployeeLookupDto[]>(`${environment.apiUrl}/employees/lookup`).subscribe({
      next: d => (this.employees = d),
      error: () => {}
    });
  }

  // ---- Goals ----
  loadGoals() {
    this.goalsLoading = true;
    this.api.getMyGoals().subscribe({
      next: d => { this.goals = d; this.goalsLoading = false; },
      error: err => { this.goalsLoading = false; this.notify.error(err.error?.message ?? 'Failed to load goals.'); }
    });
  }

  loadMyGoals() {
    this.myGoalsLoading = true;
    this.api.getMyGoals().subscribe({
      next: d => { this.myGoals = d; this.myGoalsLoading = false; },
      error: () => { this.myGoalsLoading = false; }
    });
  }

  openCreateGoal() {
    this.editingGoalId = null;
    this.goalForm = this.emptyGoalForm();
    this.showGoalDialog = true;
  }

  openEditGoal(g: PerformanceGoalDto) {
    this.editingGoalId = g.id;
    this.goalForm = {
      title: g.title, description: g.description, metric: g.metric,
      weight: g.weight, startDate: g.startDate, dueDate: g.dueDate, status: g.status
    };
    this.showGoalDialog = true;
  }

  saveGoal() {
    if (!this.goalForm.title?.trim()) { this.notify.warn('Title is required.'); return; }
    this.savingGoal = true;

    const req$ = this.editingGoalId
      ? this.api.updateGoal(this.editingGoalId, { ...this.goalForm } as UpdatePerformanceGoalDto)
      : this.api.createGoal({ ...this.goalForm } as CreatePerformanceGoalDto);

    req$.subscribe({
      next: () => {
        this.savingGoal = false; this.showGoalDialog = false;
        this.notify.success(`Goal ${this.editingGoalId ? 'updated' : 'created'}.`);
        this.loadGoals(); this.loadMyGoals();
      },
      error: err => { this.savingGoal = false; this.notify.error(err.error?.message ?? 'Failed to save goal.'); }
    });
  }

  openProgress(g: PerformanceGoalDto) {
    this.progressTarget = g;
    this.progressForm = { progressPercent: g.progressPercent, status: g.status };
    this.showProgressDialog = true;
  }

  saveProgress() {
    if (!this.progressTarget) return;
    this.savingProgress = true;
    this.api.updateGoalProgress(this.progressTarget.id, this.progressForm).subscribe({
      next: () => {
        this.savingProgress = false; this.showProgressDialog = false;
        this.notify.success('Progress updated.');
        this.loadGoals(); this.loadMyGoals();
      },
      error: err => { this.savingProgress = false; this.notify.error(err.error?.message ?? 'Failed to update progress.'); }
    });
  }

  deleteGoal(g: PerformanceGoalDto) {
    this.confirm.confirm({
      message: `Delete goal "${g.title}"?`, header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.api.deleteGoal(g.id).subscribe({
        next: () => { this.notify.success('Goal deleted.'); this.loadGoals(); this.loadMyGoals(); },
        error: err => this.notify.error(err.error?.message ?? 'Failed to delete goal.')
      })
    });
  }

  // ---- Reviews ----
  loadReviews() {
    this.reviewsLoading = true;
    this.api.getReviews(this.reviewPeriodFilter).subscribe({
      next: d => { this.reviews = d; this.reviewsLoading = false; },
      error: err => { this.reviewsLoading = false; this.notify.error(err.error?.message ?? 'Failed to load reviews.'); }
    });
  }

  loadMyReviews() {
    this.myReviewsLoading = true;
    this.api.getMyReviews().subscribe({
      next: d => { this.myReviews = d; this.myReviewsLoading = false; },
      error: () => { this.myReviewsLoading = false; }
    });
  }

  openCreateReview() {
    this.reviewForm = this.emptyReviewForm();
    this.showReviewDialog = true;
  }

  saveReview() {
    if (!this.reviewForm.employeeId || !this.reviewForm.period?.trim()) {
      this.notify.warn('Employee and period are required.'); return;
    }
    this.savingReview = true;
    this.api.createReview(this.reviewForm).subscribe({
      next: () => {
        this.savingReview = false; this.showReviewDialog = false;
        this.notify.success('Review created.'); this.loadReviews();
      },
      error: err => { this.savingReview = false; this.notify.error(err.error?.message ?? 'Failed to create review.'); }
    });
  }

  openSelfReview(r: PerformanceReviewDto) {
    this.selfReviewTarget = r;
    this.selfReviewForm = { selfRating: r.selfRating ?? 3, selfComments: r.selfComments ?? '' };
    this.showSelfReviewDialog = true;
  }

  saveSelfReview() {
    if (!this.selfReviewTarget) return;
    this.savingSelf = true;
    this.api.submitSelfReview(this.selfReviewTarget.id, this.selfReviewForm).subscribe({
      next: () => {
        this.savingSelf = false; this.showSelfReviewDialog = false;
        this.notify.success('Self review submitted.'); this.loadMyReviews();
      },
      error: err => { this.savingSelf = false; this.notify.error(err.error?.message ?? 'Failed to submit self review.'); }
    });
  }

  openManagerReview(r: PerformanceReviewDto) {
    this.managerReviewTarget = r;
    this.managerReviewForm = {
      managerRating: r.managerRating ?? 3,
      managerComments: r.managerComments ?? '',
      recommendation: r.recommendation ?? 'Retain'
    };
    this.showManagerReviewDialog = true;
  }

  saveManagerReview() {
    if (!this.managerReviewTarget) return;
    this.savingManager = true;
    this.api.submitManagerReview(this.managerReviewTarget.id, this.managerReviewForm).subscribe({
      next: () => {
        this.savingManager = false; this.showManagerReviewDialog = false;
        this.notify.success('Manager review submitted.'); this.loadReviews();
      },
      error: err => { this.savingManager = false; this.notify.error(err.error?.message ?? 'Failed to submit manager review.'); }
    });
  }

  // ---- Severities ----
  goalStatusSeverity(s: string): 'success'|'info'|'warn'|'danger'|'secondary' {
    switch (s) {
      case 'Completed':  return 'success';
      case 'InProgress': return 'info';
      case 'NotStarted': return 'secondary';
      case 'Cancelled':  return 'danger';
      default:           return 'secondary';
    }
  }

  reviewStatusSeverity(s: string): 'success'|'info'|'warn'|'secondary' {
    switch (s) {
      case 'Completed': return 'success';
      case 'SelfDone':  return 'info';
      case 'Draft':     return 'secondary';
      default:          return 'secondary';
    }
  }

  private toDateOnly(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;
  }

  private emptyGoalForm() {
    const today = this.toDateOnly(new Date());
    return { employeeId: 0, title: '', description: null as string|null, metric: null as string|null,
             weight: 100, startDate: today, dueDate: today, status: 'NotStarted' };
  }

  private emptyReviewForm(): CreatePerformanceReviewDto {
    const now = new Date();
    return { employeeId: 0, period: `${now.getFullYear()}-Q${Math.ceil((now.getMonth()+1)/3)}`, reviewerUserId: null };
  }
}
