import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { ScheduledJobApiService, ScheduledJobDefinitionDto } from '../../core/scheduled-job-api.service';

@Component({
  selector: 'app-scheduled-jobs',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, CheckboxModule, TagModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './scheduled-jobs.component.html',
  styleUrl: './scheduled-jobs.component.scss'
})
export class ScheduledJobsComponent implements OnInit {
  jobs: ScheduledJobDefinitionDto[] = [];
  loading = false;

  editingId: number | null = null;
  editCron = '';
  editEnabled = false;
  saving = false;
  triggeringId: number | null = null;

  constructor(
    private api: ScheduledJobApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.jobs = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load scheduled jobs.');
      }
    });
  }

  startEdit(job: ScheduledJobDefinitionDto) {
    this.editingId = job.id;
    this.editCron = job.cronExpression;
    this.editEnabled = job.isEnabled;
  }

  cancelEdit() {
    this.editingId = null;
  }

  saveEdit(job: ScheduledJobDefinitionDto) {
    if (!this.editCron) {
      this.notify.warn('Cron expression is required.');
      return;
    }
    this.saving = true;
    this.api.update(job.id, { cronExpression: this.editCron, isEnabled: this.editEnabled }).subscribe({
      next: () => {
        this.saving = false;
        this.editingId = null;
        this.notify.success('Scheduled job updated.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update scheduled job.');
      }
    });
  }

  triggerNow(job: ScheduledJobDefinitionDto) {
    this.triggeringId = job.id;
    this.api.triggerNow(job.id).subscribe({
      next: () => {
        this.triggeringId = null;
        this.notify.success(`"${job.name}" queued to run now.`);
        this.load();
      },
      error: err => {
        this.triggeringId = null;
        this.notify.error(err.error?.message ?? 'Failed to trigger job.');
      }
    });
  }
}
