import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  LeaveTypeApiService, LeaveTypeDto, CreateLeaveTypeDto, UpdateLeaveTypeDto
} from '../../../core/leave-type-api.service';

@Component({
  selector: 'app-leave-types',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './leave-types.component.html',
  styleUrl: './leave-types.component.scss'
})
export class LeaveTypesComponent implements OnInit {
  leaveTypes: LeaveTypeDto[] = [];
  loading = false;

  showDialog = false;
  editing: LeaveTypeDto | null = null;
  saving = false;

  form: { name: string; code: string; defaultDaysPerYear: number | null; isActive: boolean } = this.emptyForm();

  constructor(
    private api: LeaveTypeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { name: '', code: '', defaultDaysPerYear: null, isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.leaveTypes = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave types.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(leaveType: LeaveTypeDto) {
    this.editing = leaveType;
    this.form = {
      name: leaveType.name, code: leaveType.code,
      defaultDaysPerYear: leaveType.defaultDaysPerYear, isActive: leaveType.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      defaultDaysPerYear: this.form.defaultDaysPerYear,
      isActive: this.form.isActive
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateLeaveTypeDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateLeaveTypeDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Leave type ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save leave type.');
      }
    });
  }

  delete(leaveType: LeaveTypeDto) {
    this.confirm.confirm({
      message: `Delete leave type "${leaveType.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(leaveType.id).subscribe({
          next: () => {
            this.notify.success('Leave type deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete leave type.')
        });
      }
    });
  }
}
