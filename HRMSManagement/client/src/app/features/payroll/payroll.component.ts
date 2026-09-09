import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  PayrollApiService,
  SalaryComponentDto,
  CreateSalaryComponentDto,
  UpdateSalaryComponentDto,
  PayrollRunDto,
  PayslipDto,
  EmployeeSalaryDto
} from '../../core/payroll-api.service';

interface ComponentForm {
  name: string;
  code: string;
  type: string;
  calcType: string;
  defaultValue: number;
  isTaxable: boolean;
  isStatutory: boolean;
  displayOrder: number;
  isActive: boolean;
}

interface RunForm {
  year: number;
  month: number;
  notes: string;
}

const MONTH_OPTIONS = [
  { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
  { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
  { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
  { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
];

const TYPE_OPTIONS = [
  { label: 'Earning', value: 'Earning' },
  { label: 'Deduction', value: 'Deduction' }
];

const CALC_TYPE_OPTIONS = [
  { label: 'Fixed', value: 'Fixed' },
  { label: 'Percent of Basic', value: 'PercentOfBasic' }
];

function emptyComponentForm(): ComponentForm {
  return {
    name: '', code: '', type: 'Earning', calcType: 'Fixed',
    defaultValue: 0, isTaxable: true, isStatutory: false, displayOrder: 0, isActive: true
  };
}

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    TableModule, DialogModule, ButtonModule, InputTextModule, DropdownModule,
    TagModule, TabViewModule, InputNumberModule, CheckboxModule, TooltipModule,
    ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './payroll.component.html',
  styleUrl: './payroll.component.scss'
})
export class PayrollComponent implements OnInit {
  monthOptions = MONTH_OPTIONS;
  typeOptions = TYPE_OPTIONS;
  calcTypeOptions = CALC_TYPE_OPTIONS;

  // ---------- Salary components ----------
  components = signal<SalaryComponentDto[]>([]);
  loadingComponents = signal(false);
  componentDialogVisible = signal(false);
  editingComponentId = signal<number | null>(null);
  componentForm: ComponentForm = emptyComponentForm();

  // ---------- Payroll runs ----------
  runs = signal<PayrollRunDto[]>([]);
  loadingRuns = signal(false);
  newRunDialogVisible = signal(false);
  runForm: RunForm = { year: new Date().getFullYear(), month: new Date().getMonth() + 1, notes: '' };
  runActionLoading = signal<number | null>(null);

  payslipsDialogVisible = signal(false);
  payslipsForRun = signal<PayrollRunDto | null>(null);
  runPayslips = signal<PayslipDto[]>([]);
  loadingRunPayslips = signal(false);

  // ---------- My payslips ----------
  myPayslips = signal<PayslipDto[]>([]);
  loadingMyPayslips = signal(false);
  mySalary = signal<EmployeeSalaryDto | null>(null);
  loadingMySalary = signal(false);

  payslipDetailVisible = signal(false);
  selectedPayslip = signal<PayslipDto | null>(null);
  loadingPayslipDetail = signal(false);

  constructor(
    private api: PayrollApiService,
    private notify: NotificationService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadComponents();
    this.loadRuns();
    this.loadMyPayslips();
    this.loadMySalary();
  }

  monthName(month: number): string {
    return this.monthOptions.find(m => m.value === month)?.label ?? String(month);
  }

  // ================= Salary components =================

  loadComponents() {
    this.loadingComponents.set(true);
    this.api.getComponents().subscribe({
      next: data => { this.components.set(data); this.loadingComponents.set(false); },
      error: err => { this.loadingComponents.set(false); this.notify.error(err.error?.message ?? 'Failed to load salary components.'); }
    });
  }

  openAddComponent() {
    this.editingComponentId.set(null);
    this.componentForm = emptyComponentForm();
    this.componentDialogVisible.set(true);
  }

  openEditComponent(c: SalaryComponentDto) {
    this.editingComponentId.set(c.id);
    this.componentForm = {
      name: c.name, code: c.code, type: c.type, calcType: c.calcType,
      defaultValue: c.defaultValue, isTaxable: c.isTaxable, isStatutory: c.isStatutory,
      displayOrder: c.displayOrder, isActive: c.isActive
    };
    this.componentDialogVisible.set(true);
  }

  saveComponent() {
    if (!this.componentForm.name || !this.componentForm.code) {
      this.notify.warn('Name and code are required.');
      return;
    }
    const id = this.editingComponentId();
    if (id) {
      const dto: UpdateSalaryComponentDto = { ...this.componentForm };
      this.api.updateComponent(id, dto).subscribe({
        next: () => {
          this.notify.success('Salary component updated.');
          this.componentDialogVisible.set(false);
          this.loadComponents();
        },
        error: err => this.notify.error(err.error?.message ?? 'Failed to update salary component.')
      });
    } else {
      const dto: CreateSalaryComponentDto = {
        name: this.componentForm.name, code: this.componentForm.code, type: this.componentForm.type,
        calcType: this.componentForm.calcType, defaultValue: this.componentForm.defaultValue,
        isTaxable: this.componentForm.isTaxable, isStatutory: this.componentForm.isStatutory,
        displayOrder: this.componentForm.displayOrder
      };
      this.api.createComponent(dto).subscribe({
        next: () => {
          this.notify.success('Salary component created.');
          this.componentDialogVisible.set(false);
          this.loadComponents();
        },
        error: err => this.notify.error(err.error?.message ?? 'Failed to create salary component.')
      });
    }
  }

  deleteComponent(c: SalaryComponentDto) {
    this.confirmationService.confirm({
      header: 'Delete Salary Component',
      message: `Delete salary component "${c.name}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteComponent(c.id).subscribe({
          next: () => {
            this.notify.success('Salary component deleted.');
            this.loadComponents();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete salary component.')
        });
      }
    });
  }

  // ================= Payroll runs =================

  loadRuns() {
    this.loadingRuns.set(true);
    this.api.getRuns().subscribe({
      next: data => { this.runs.set(data); this.loadingRuns.set(false); },
      error: err => { this.loadingRuns.set(false); this.notify.error(err.error?.message ?? 'Failed to load payroll runs.'); }
    });
  }

  runStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Draft': return 'secondary';
      case 'Processing': return 'info';
      case 'Completed': return 'success';
      case 'Locked': return 'warn';
      case 'Paid': return 'success';
      default: return 'secondary';
    }
  }

  openNewRunDialog() {
    this.runForm = { year: new Date().getFullYear(), month: new Date().getMonth() + 1, notes: '' };
    this.newRunDialogVisible.set(true);
  }

  createRun() {
    this.api.createRun({ year: this.runForm.year, month: this.runForm.month, notes: this.runForm.notes || null }).subscribe({
      next: () => {
        this.notify.success('Payroll run created.');
        this.newRunDialogVisible.set(false);
        this.loadRuns();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to create payroll run.')
    });
  }

  processRun(run: PayrollRunDto) {
    this.runActionLoading.set(run.id);
    this.api.processRun(run.id).subscribe({
      next: () => {
        this.runActionLoading.set(null);
        this.notify.success(`Payroll run ${this.monthName(run.month)} ${run.year} processed.`);
        this.loadRuns();
      },
      error: err => { this.runActionLoading.set(null); this.notify.error(err.error?.message ?? 'Failed to process payroll run.'); }
    });
  }

  lockRun(run: PayrollRunDto) {
    this.confirmationService.confirm({
      header: 'Lock Payroll Run',
      message: `Lock the ${this.monthName(run.month)} ${run.year} payroll run? Locked runs cannot be reprocessed.`,
      icon: 'pi pi-lock',
      accept: () => {
        this.runActionLoading.set(run.id);
        this.api.lockRun(run.id).subscribe({
          next: () => { this.runActionLoading.set(null); this.notify.success('Payroll run locked.'); this.loadRuns(); },
          error: err => { this.runActionLoading.set(null); this.notify.error(err.error?.message ?? 'Failed to lock payroll run.'); }
        });
      }
    });
  }

  markPaid(run: PayrollRunDto) {
    this.confirmationService.confirm({
      header: 'Mark as Paid',
      message: `Mark the ${this.monthName(run.month)} ${run.year} payroll run as paid?`,
      icon: 'pi pi-check-circle',
      accept: () => {
        this.runActionLoading.set(run.id);
        this.api.markPaid(run.id).subscribe({
          next: () => { this.runActionLoading.set(null); this.notify.success('Payroll run marked as paid.'); this.loadRuns(); },
          error: err => { this.runActionLoading.set(null); this.notify.error(err.error?.message ?? 'Failed to mark payroll run as paid.'); }
        });
      }
    });
  }

  deleteRun(run: PayrollRunDto) {
    this.confirmationService.confirm({
      header: 'Delete Payroll Run',
      message: `Delete the ${this.monthName(run.month)} ${run.year} payroll run? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.api.deleteRun(run.id).subscribe({
          next: () => { this.notify.success('Payroll run deleted.'); this.loadRuns(); },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete payroll run.')
        });
      }
    });
  }

  canDeleteRun(run: PayrollRunDto): boolean {
    return run.status === 'Draft' || run.status === 'Completed';
  }

  downloadBankFile(run: PayrollRunDto) {
    this.api.getBankFile(run.id).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `bank-file-${run.year}-${run.month}.csv`;
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        window.URL.revokeObjectURL(url);
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to download bank file.')
    });
  }

  viewPayslips(run: PayrollRunDto) {
    this.payslipsForRun.set(run);
    this.payslipsDialogVisible.set(true);
    this.loadingRunPayslips.set(true);
    this.api.getPayslipsByRun(run.id).subscribe({
      next: data => { this.runPayslips.set(data); this.loadingRunPayslips.set(false); },
      error: err => { this.loadingRunPayslips.set(false); this.notify.error(err.error?.message ?? 'Failed to load payslips.'); }
    });
  }

  // ================= My payslips =================

  loadMyPayslips() {
    this.loadingMyPayslips.set(true);
    this.api.getMyPayslips().subscribe({
      next: data => { this.myPayslips.set(data); this.loadingMyPayslips.set(false); },
      error: err => { this.loadingMyPayslips.set(false); this.notify.error(err.error?.message ?? 'Failed to load your payslips.'); }
    });
  }

  loadMySalary() {
    this.loadingMySalary.set(true);
    this.api.getMySalary().subscribe({
      next: data => { this.mySalary.set(data); this.loadingMySalary.set(false); },
      error: () => { this.loadingMySalary.set(false); }
    });
  }

  viewPayslipDetail(payslip: PayslipDto) {
    this.payslipDetailVisible.set(true);
    this.loadingPayslipDetail.set(true);
    this.api.getMyPayslip(payslip.id).subscribe({
      next: data => { this.selectedPayslip.set(data); this.loadingPayslipDetail.set(false); },
      error: err => {
        this.loadingPayslipDetail.set(false);
        this.selectedPayslip.set(payslip);
        this.notify.error(err.error?.message ?? 'Failed to load payslip details.');
      }
    });
  }
}
