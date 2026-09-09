import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { environment } from '../../../environments/environment';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  RecruitmentApiService,
  JobOpeningDto,
  CreateJobOpeningDto,
  UpdateJobOpeningDto,
  CandidateDto,
  CandidateDetailDto,
  CreateCandidateDto,
  InterviewDto,
  CreateInterviewDto,
  InterviewFeedbackDto,
  RecruitmentPipelineDto
} from '../../core/recruitment-api.service';
import { CandidateStageProgress } from '../../core/candidate-stage-progress';

interface LookupOption {
  id: number;
  name: string;
}

@Component({
  selector: 'app-recruitment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TabViewModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    InputNumberModule,
    DropdownModule,
    TagModule,
    CalendarModule,
    RatingModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './recruitment.component.html',
  styleUrl: './recruitment.component.scss'
})
export class RecruitmentComponent implements OnInit {
  activeTabIndex = 0;

  // Lookups
  departments: LookupOption[] = [];
  designations: LookupOption[] = [];

  employmentTypeOptions = [
    { label: 'Full Time', value: 'FullTime' },
    { label: 'Part Time', value: 'PartTime' },
    { label: 'Contract', value: 'Contract' },
    { label: 'Intern', value: 'Intern' }
  ];

  openingStatusOptions = [
    { label: 'Open', value: 'Open' },
    { label: 'On Hold', value: 'OnHold' },
    { label: 'Closed', value: 'Closed' }
  ];

  stageOptions = [
    { label: 'Applied', value: 'Applied' },
    { label: 'Screening', value: 'Screening' },
    { label: 'Interview', value: 'Interview' },
    { label: 'Offered', value: 'Offered' },
    { label: 'Hired', value: 'Hired' },
    { label: 'Rejected', value: 'Rejected' }
  ];

  interviewModeOptions = [
    { label: 'Video', value: 'Video' },
    { label: 'Phone', value: 'Phone' },
    { label: 'In Person', value: 'InPerson' }
  ];

  interviewOutcomeOptions = [
    { label: 'Completed', value: 'Completed' },
    { label: 'Cancelled', value: 'Cancelled' },
    { label: 'No Show', value: 'NoShow' }
  ];

  // Openings
  openings: JobOpeningDto[] = [];
  openingsLoading = false;
  showOpeningDialog = false;
  savingOpening = false;
  editingOpeningId: number | null = null;
  openingForm: UpdateJobOpeningDto = this.emptyOpeningForm();

  // Candidates
  selectedOpening: JobOpeningDto | null = null;
  candidates: CandidateDto[] = [];
  candidatesLoading = false;
  showCandidateDialog = false;
  savingCandidate = false;
  candidateForm: CreateCandidateDto = this.emptyCandidateForm();

  // Stage change (Offered) dialog
  showStageDialog = false;
  stageTarget: CandidateDto | null = null;
  savingStage = false;
  stageForm = {
    stage: 'Offered',
    notes: '' as string | null,
    offeredSalary: null as number | null,
    offerDate: new Date() as Date | null,
    expectedJoiningDate: null as Date | null
  };

  // Candidate detail / interviews
  showCandidateDetailDialog = false;
  candidateDetail: CandidateDetailDto | null = null;
  candidateDetailLoading = false;

  showInterviewDialog = false;
  savingInterview = false;
  interviewForm = this.emptyInterviewForm();

  showFeedbackDialog = false;
  savingFeedback = false;
  feedbackTarget: InterviewDto | null = null;
  feedbackForm: InterviewFeedbackDto = { status: 'Completed', feedback: '', score: null };

  // Pipeline
  pipeline: RecruitmentPipelineDto | null = null;
  pipelineLoading = false;

  // Org-wide interviews
  upcomingInterviews: InterviewDto[] = [];
  upcomingInterviewsLoading = false;

  // Public careers portal
  orgCode = '';

  constructor(
    private api: RecruitmentApiService,
    private http: HttpClient,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.loadOpenings();
    this.loadPipeline();
    this.loadUpcomingInterviews();
    this.loadOrgCode();
  }

  loadLookups() {
    this.http.get<LookupOption[]>(`${environment.apiUrl}/departments`).subscribe({
      next: data => (this.departments = data),
      error: () => (this.departments = [])
    });
    this.http.get<{ id: number; title: string }[]>(`${environment.apiUrl}/designations`).subscribe({
      next: data => (this.designations = data.map(d => ({ id: d.id, name: d.title }))),
      error: () => (this.designations = [])
    });
  }

  loadOrgCode() {
    this.http.get<{ code: string }>(`${environment.apiUrl}/organization`).subscribe({
      next: org => (this.orgCode = org.code),
      error: () => (this.orgCode = '')
    });
  }

  publicApplyLink(opening: JobOpeningDto): string {
    return `${window.location.origin}/careers/${this.orgCode}/jobs/${opening.id}`;
  }

  copyApplyLink(opening: JobOpeningDto) {
    if (!this.orgCode) {
      this.notify.warn('Organization code not loaded yet — try again in a moment.');
      return;
    }
    navigator.clipboard?.writeText(this.publicApplyLink(opening));
    this.notify.success('Apply link copied — paste it into your LinkedIn/Naukri job post.');
  }

  // ---------------- Job Openings ----------------
  loadOpenings() {
    this.openingsLoading = true;
    this.api.getOpenings().subscribe({
      next: data => {
        this.openings = data;
        this.openingsLoading = false;
      },
      error: err => {
        this.openingsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load job openings.');
      }
    });
  }

  openAddOpening() {
    this.editingOpeningId = null;
    this.openingForm = this.emptyOpeningForm();
    this.showOpeningDialog = true;
  }

  editOpening(o: JobOpeningDto) {
    this.editingOpeningId = o.id;
    this.openingForm = {
      departmentId: o.departmentId,
      designationId: o.designationId,
      title: o.title,
      description: o.description,
      vacancies: o.vacancies,
      location: o.location,
      employmentType: o.employmentType,
      minExperienceYears: o.minExperienceYears,
      maxExperienceYears: o.maxExperienceYears,
      salaryRangeFrom: o.salaryRangeFrom,
      salaryRangeTo: o.salaryRangeTo,
      status: o.status,
      postedDate: o.postedDate,
      closingDate: o.closingDate
    };
    this.showOpeningDialog = true;
  }

  saveOpening() {
    if (!this.openingForm.title || !this.openingForm.departmentId || !this.openingForm.designationId) {
      this.notify.warn('Please fill in title, department and designation.');
      return;
    }
    this.savingOpening = true;
    const req$ = this.editingOpeningId
      ? this.api.updateOpening(this.editingOpeningId, this.openingForm)
      : this.api.createOpening(this.openingForm as CreateJobOpeningDto);
    req$.subscribe({
      next: () => {
        this.savingOpening = false;
        this.showOpeningDialog = false;
        this.notify.success(`Job opening ${this.editingOpeningId ? 'updated' : 'created'} successfully.`);
        this.loadOpenings();
      },
      error: err => {
        this.savingOpening = false;
        this.notify.error(err.error?.message ?? 'Failed to save job opening.');
      }
    });
  }

  confirmDeleteOpening(o: JobOpeningDto) {
    this.confirmation.confirm({
      message: `Delete job opening "${o.title}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteOpening(o)
    });
  }

  deleteOpening(o: JobOpeningDto) {
    this.api.deleteOpening(o.id).subscribe({
      next: () => {
        this.notify.success('Job opening deleted.');
        if (this.selectedOpening?.id === o.id) this.selectedOpening = null;
        this.loadOpenings();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete job opening.')
    });
  }

  selectOpening(o: JobOpeningDto) {
    this.selectedOpening = o;
    this.activeTabIndex = 1;
    this.loadCandidates(o.id);
  }

  openingStatusSeverity(status: string): 'success' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Open': return 'success';
      case 'OnHold': return 'warn';
      case 'Closed': return 'danger';
      default: return 'secondary';
    }
  }

  // ---------------- Candidates ----------------
  loadCandidates(openingId: number) {
    this.candidatesLoading = true;
    this.api.getCandidates(openingId).subscribe({
      next: data => {
        this.candidates = data;
        this.candidatesLoading = false;
      },
      error: err => {
        this.candidatesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load candidates.');
      }
    });
  }

  openAddCandidate() {
    if (!this.selectedOpening) {
      this.notify.warn('Select a job opening first.');
      return;
    }
    this.candidateForm = this.emptyCandidateForm();
    this.showCandidateDialog = true;
  }

  saveCandidate() {
    if (!this.selectedOpening) return;
    if (!this.candidateForm.name || !this.candidateForm.email) {
      this.notify.warn('Please fill in name and email.');
      return;
    }
    this.savingCandidate = true;
    this.api.createCandidate(this.selectedOpening.id, this.candidateForm).subscribe({
      next: () => {
        this.savingCandidate = false;
        this.showCandidateDialog = false;
        this.notify.success('Candidate added successfully.');
        this.loadCandidates(this.selectedOpening!.id);
        this.loadOpenings();
      },
      error: err => {
        this.savingCandidate = false;
        this.notify.error(err.error?.message ?? 'Failed to add candidate.');
      }
    });
  }

  stageSeverity(stage: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (stage) {
      case 'Applied': return 'secondary';
      case 'Screening': return 'info';
      case 'Interview': return 'warn';
      case 'Offered': return 'info';
      case 'Hired': return 'success';
      case 'Rejected': return 'danger';
      default: return 'secondary';
    }
  }

  onStageSelect(candidate: CandidateDto, newStage: string) {
    if (!newStage || newStage === candidate.stage) return;
    if (newStage === 'Offered') {
      this.stageTarget = candidate;
      this.stageForm = {
        stage: 'Offered',
        notes: '',
        offeredSalary: candidate.expectedSalary ?? null,
        offerDate: new Date(),
        expectedJoiningDate: null
      };
      this.showStageDialog = true;
      return;
    }
    this.applyStageChange(candidate, { stage: newStage });
  }

  submitStageChange() {
    if (!this.stageTarget) return;
    this.savingStage = true;
    this.api
      .changeStage(this.stageTarget.id, {
        stage: this.stageForm.stage,
        notes: this.stageForm.notes,
        offeredSalary: this.stageForm.offeredSalary,
        offerDate: this.stageForm.offerDate ? this.toDateOnly(this.stageForm.offerDate) : null,
        expectedJoiningDate: this.stageForm.expectedJoiningDate ? this.toDateOnly(this.stageForm.expectedJoiningDate) : null
      })
      .subscribe({
        next: () => {
          this.savingStage = false;
          this.showStageDialog = false;
          this.notify.success('Candidate stage updated.');
          if (this.selectedOpening) this.loadCandidates(this.selectedOpening.id);
        },
        error: err => {
          this.savingStage = false;
          this.notify.error(err.error?.message ?? 'Failed to update candidate stage.');
        }
      });
  }

  private applyStageChange(candidate: CandidateDto, dto: { stage: string }) {
    this.api.changeStage(candidate.id, dto).subscribe({
      next: () => {
        this.notify.success('Candidate stage updated.');
        if (this.selectedOpening) this.loadCandidates(this.selectedOpening.id);
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to update candidate stage.')
    });
  }

  confirmDeleteCandidate(c: CandidateDto) {
    this.confirmation.confirm({
      message: `Delete candidate "${c.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteCandidate(c)
    });
  }

  deleteCandidate(c: CandidateDto) {
    this.api.deleteCandidate(c.id).subscribe({
      next: () => {
        this.notify.success('Candidate deleted.');
        if (this.selectedOpening) this.loadCandidates(this.selectedOpening.id);
        this.loadOpenings();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete candidate.')
    });
  }

  // ---------------- Candidate Detail / Interviews ----------------
  openCandidateDetail(c: CandidateDto) {
    this.candidateDetailLoading = true;
    this.showCandidateDetailDialog = true;
    this.api.getCandidate(c.id).subscribe({
      next: detail => {
        this.candidateDetail = detail;
        this.candidateDetailLoading = false;
      },
      error: err => {
        this.candidateDetailLoading = false;
        this.showCandidateDetailDialog = false;
        this.notify.error(err.error?.message ?? 'Failed to load candidate details.');
      }
    });
  }

  private refreshCandidateDetail() {
    if (!this.candidateDetail) return;
    this.api.getCandidate(this.candidateDetail.id).subscribe({
      next: detail => (this.candidateDetail = detail),
      error: () => {}
    });
  }

  openAddInterview() {
    if (!this.candidateDetail) return;
    this.interviewForm = this.emptyInterviewForm();
    this.showInterviewDialog = true;
  }

  saveInterview() {
    if (!this.candidateDetail) return;
    if (!this.interviewForm.title || !this.interviewForm.scheduledAt) {
      this.notify.warn('Please fill in title and scheduled date/time.');
      return;
    }
    this.savingInterview = true;
    const dto: CreateInterviewDto = {
      round: this.interviewForm.round,
      title: this.interviewForm.title,
      scheduledAt: this.interviewForm.scheduledAt.toISOString(),
      mode: this.interviewForm.mode,
      interviewerName: this.interviewForm.interviewerName,
      interviewerUserId: this.interviewForm.interviewerUserId
    };
    this.api.scheduleInterview(this.candidateDetail.id, dto).subscribe({
      next: () => {
        this.savingInterview = false;
        this.showInterviewDialog = false;
        this.notify.success('Interview scheduled.');
        this.refreshCandidateDetail();
        this.loadUpcomingInterviews();
      },
      error: err => {
        this.savingInterview = false;
        this.notify.error(err.error?.message ?? 'Failed to schedule interview.');
      }
    });
  }

  openFeedback(interview: InterviewDto) {
    this.feedbackTarget = interview;
    this.feedbackForm = {
      status: interview.status === 'Scheduled' ? 'Completed' : interview.status,
      feedback: interview.feedback ?? '',
      score: interview.score ?? null
    };
    this.showFeedbackDialog = true;
  }

  submitFeedback() {
    if (!this.feedbackTarget) return;
    this.savingFeedback = true;
    this.api.submitInterviewFeedback(this.feedbackTarget.id, this.feedbackForm).subscribe({
      next: () => {
        this.savingFeedback = false;
        this.showFeedbackDialog = false;
        this.notify.success('Interview feedback submitted.');
        this.refreshCandidateDetail();
        this.loadUpcomingInterviews();
      },
      error: err => {
        this.savingFeedback = false;
        this.notify.error(err.error?.message ?? 'Failed to submit feedback.');
      }
    });
  }

  interviewStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Scheduled': return 'info';
      case 'Completed': return 'success';
      case 'Cancelled': return 'danger';
      case 'NoShow': return 'warn';
      default: return 'secondary';
    }
  }

  // ---------------- Progress stepper (candidate detail) ----------------
  get candidateStagePoints() {
    if (!this.candidateDetail) return [];
    return CandidateStageProgress.stepperPoints(
      this.candidateDetail.stage,
      this.candidateDetail.stageHistory.map(h => h.stage)
    );
  }

  stageLabel(stage: string): string {
    return CandidateStageProgress.labels[stage] ?? stage;
  }

  // ---------------- Pipeline ----------------
  loadPipeline() {
    this.pipelineLoading = true;
    this.api.getPipeline().subscribe({
      next: p => {
        this.pipeline = p;
        this.pipelineLoading = false;
      },
      error: err => {
        this.pipelineLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load pipeline.');
      }
    });
  }

  get pipelineStages(): { label: string; value: number }[] {
    if (!this.pipeline) return [];
    return [
      { label: 'Applied', value: this.pipeline.applied },
      { label: 'Screening', value: this.pipeline.screening },
      { label: 'Interview', value: this.pipeline.interview },
      { label: 'Offered', value: this.pipeline.offered },
      { label: 'Hired', value: this.pipeline.hired },
      { label: 'Rejected', value: this.pipeline.rejected }
    ];
  }

  // ---------------- Org-wide interviews ----------------
  loadUpcomingInterviews() {
    this.upcomingInterviewsLoading = true;
    this.api.getUpcomingInterviews().subscribe({
      next: data => {
        this.upcomingInterviews = data;
        this.upcomingInterviewsLoading = false;
      },
      error: err => {
        this.upcomingInterviewsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load interviews.');
      }
    });
  }

  // ---------------- Helpers ----------------
  private toDateOnly(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private emptyOpeningForm(): UpdateJobOpeningDto {
    return {
      departmentId: 0,
      designationId: 0,
      title: '',
      description: '',
      vacancies: 1,
      location: '',
      employmentType: 'FullTime',
      minExperienceYears: null,
      maxExperienceYears: null,
      salaryRangeFrom: null,
      salaryRangeTo: null,
      status: 'Open',
      postedDate: this.toDateOnly(new Date()),
      closingDate: null
    };
  }

  private emptyCandidateForm(): CreateCandidateDto {
    return {
      name: '',
      email: '',
      phone: '',
      resumeUrl: '',
      currentCompany: '',
      totalExperienceYears: null,
      expectedSalary: null,
      source: '',
      rating: null,
      notes: ''
    };
  }

  private emptyInterviewForm() {
    return {
      round: 1,
      title: '',
      scheduledAt: new Date(),
      mode: 'Video',
      interviewerName: '' as string | null,
      interviewerUserId: null as number | null
    };
  }
}
