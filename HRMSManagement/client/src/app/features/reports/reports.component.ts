import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';

import { NotificationService } from '../../core/notification.service';
import {
  ReportApiService,
  HeadcountReportRow,
  AttendanceMonthlyReportRow,
  LeaveReportRow,
  PayrollSummaryReportRow,
  RecruitmentPipelineReportRow,
  AssetReportRow
} from '../../core/report-api.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, TabViewModule, TableModule, ButtonModule, DropdownModule, TagModule, ToastModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  years: number[] = [];
  months = [
    { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
    { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
    { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
    { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
  ];

  // Headcount
  headcountRows: HeadcountReportRow[] = [];
  headcountLoading = false;

  // Attendance
  attendanceYear = new Date().getFullYear();
  attendanceMonth = new Date().getMonth() + 1;
  attendanceRows: AttendanceMonthlyReportRow[] = [];
  attendanceLoading = false;

  // Leave
  leaveYear = new Date().getFullYear();
  leaveRows: LeaveReportRow[] = [];
  leaveLoading = false;

  // Payroll Summary
  payrollYear = new Date().getFullYear();
  payrollRows: PayrollSummaryReportRow[] = [];
  payrollLoading = false;

  // Recruitment Pipeline
  recruitmentRows: RecruitmentPipelineReportRow[] = [];
  recruitmentLoading = false;

  // Assets
  assetRows: AssetReportRow[] = [];
  assetLoading = false;

  constructor(private api: ReportApiService, private notify: NotificationService) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 3; y <= currentYear; y++) this.years.push(y);
  }

  ngOnInit() {
    this.loadHeadcount();
    this.loadAttendance();
    this.loadLeave();
    this.loadPayrollSummary();
    this.loadRecruitmentPipeline();
    this.loadAssets();
  }

  private triggerDownload(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  // ---------- Headcount ----------
  loadHeadcount() {
    this.headcountLoading = true;
    this.api.getHeadcount().subscribe({
      next: rows => { this.headcountRows = rows; this.headcountLoading = false; },
      error: err => {
        this.headcountLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load headcount report.');
      }
    });
  }

  downloadHeadcount() {
    this.api.downloadHeadcount().subscribe({
      next: blob => this.triggerDownload(blob, 'headcount.csv'),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download headcount report.')
    });
  }

  // ---------- Attendance ----------
  loadAttendance() {
    this.attendanceLoading = true;
    this.api.getAttendanceMonthly(this.attendanceYear, this.attendanceMonth).subscribe({
      next: rows => { this.attendanceRows = rows; this.attendanceLoading = false; },
      error: err => {
        this.attendanceLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load attendance report.');
      }
    });
  }

  downloadAttendance() {
    this.api.downloadAttendanceMonthly(this.attendanceYear, this.attendanceMonth).subscribe({
      next: blob => this.triggerDownload(blob, `attendance-${this.attendanceYear}-${this.attendanceMonth}.csv`),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download attendance report.')
    });
  }

  // ---------- Leave ----------
  loadLeave() {
    this.leaveLoading = true;
    this.api.getLeave(this.leaveYear).subscribe({
      next: rows => { this.leaveRows = rows; this.leaveLoading = false; },
      error: err => {
        this.leaveLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave report.');
      }
    });
  }

  downloadLeave() {
    this.api.downloadLeave(this.leaveYear).subscribe({
      next: blob => this.triggerDownload(blob, `leave-${this.leaveYear}.csv`),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download leave report.')
    });
  }

  // ---------- Payroll Summary ----------
  loadPayrollSummary() {
    this.payrollLoading = true;
    this.api.getPayrollSummary(this.payrollYear).subscribe({
      next: rows => { this.payrollRows = rows; this.payrollLoading = false; },
      error: err => {
        this.payrollLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load payroll summary report.');
      }
    });
  }

  downloadPayrollSummary() {
    this.api.downloadPayrollSummary(this.payrollYear).subscribe({
      next: blob => this.triggerDownload(blob, `payroll-summary-${this.payrollYear}.csv`),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download payroll summary report.')
    });
  }

  // ---------- Recruitment Pipeline ----------
  loadRecruitmentPipeline() {
    this.recruitmentLoading = true;
    this.api.getRecruitmentPipeline().subscribe({
      next: rows => { this.recruitmentRows = rows; this.recruitmentLoading = false; },
      error: err => {
        this.recruitmentLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load recruitment pipeline report.');
      }
    });
  }

  downloadRecruitmentPipeline() {
    this.api.downloadRecruitmentPipeline().subscribe({
      next: blob => this.triggerDownload(blob, 'recruitment-pipeline.csv'),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download recruitment pipeline report.')
    });
  }

  // ---------- Assets ----------
  loadAssets() {
    this.assetLoading = true;
    this.api.getAssets().subscribe({
      next: rows => { this.assetRows = rows; this.assetLoading = false; },
      error: err => {
        this.assetLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load assets report.');
      }
    });
  }

  downloadAssets() {
    this.api.downloadAssets().subscribe({
      next: blob => this.triggerDownload(blob, 'assets.csv'),
      error: err => this.notify.error(err.error?.message ?? 'Failed to download assets report.')
    });
  }
}
