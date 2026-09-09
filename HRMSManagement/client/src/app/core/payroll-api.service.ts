import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SalaryComponentDto {
  id: number;
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

export interface CreateSalaryComponentDto {
  name: string;
  code: string;
  type: string;
  calcType: string;
  defaultValue: number;
  isTaxable: boolean;
  isStatutory: boolean;
  displayOrder: number;
}

export interface UpdateSalaryComponentDto extends CreateSalaryComponentDto {
  isActive: boolean;
}

export interface EmployeeSalaryItemDto {
  componentId: number;
  componentName: string;
  type: string;
  monthlyAmount: number;
}

export interface EmployeeSalaryDto {
  id: number;
  employeeId: number;
  employeeName: string;
  effectiveFrom: string;
  annualCtc: number;
  monthlyGross: number;
  currency: string;
  notes?: string | null;
  items: EmployeeSalaryItemDto[];
}

export interface SetEmployeeSalaryItemDto {
  salaryComponentId: number;
  monthlyAmount: number;
}

export interface SetEmployeeSalaryDto {
  employeeId: number;
  effectiveFrom: string;
  annualCtc: number;
  currency: string;
  notes?: string | null;
  items: SetEmployeeSalaryItemDto[];
}

export interface PayrollRunDto {
  id: number;
  year: number;
  month: number;
  status: string;
  processedAt?: string | null;
  processedByName?: string | null;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
  payslipCount: number;
  notes?: string | null;
}

export interface CreatePayrollRunDto {
  year: number;
  month: number;
  notes?: string | null;
}

export interface PayslipItemDto {
  componentName: string;
  type: string;
  amount: number;
}

export interface PayslipDto {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  designationTitle: string;
  year: number;
  month: number;
  grossEarnings: number;
  totalDeductions: number;
  netPay: number;
  workingDays: number;
  presentDays: number;
  paidLeaveDays: number;
  lopDays: number;
  status: string;
  items: PayslipItemDto[];
}

export interface PayrollRunDetail {
  run: PayrollRunDto;
  payslips: PayslipDto[];
}

@Injectable({ providedIn: 'root' })
export class PayrollApiService {
  private baseUrl = `${environment.apiUrl}/payroll`;

  constructor(private http: HttpClient) {}

  // ---------- Salary components ----------

  getComponents(): Observable<SalaryComponentDto[]> {
    return this.http.get<SalaryComponentDto[]>(`${this.baseUrl}/components`);
  }

  createComponent(dto: CreateSalaryComponentDto): Observable<SalaryComponentDto> {
    return this.http.post<SalaryComponentDto>(`${this.baseUrl}/components`, dto);
  }

  updateComponent(id: number, dto: UpdateSalaryComponentDto): Observable<SalaryComponentDto> {
    return this.http.put<SalaryComponentDto>(`${this.baseUrl}/components/${id}`, dto);
  }

  deleteComponent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/components/${id}`);
  }

  // ---------- Employee salaries ----------

  getEmployeeSalary(employeeId: number): Observable<EmployeeSalaryDto> {
    return this.http.get<EmployeeSalaryDto>(`${this.baseUrl}/salaries/employee/${employeeId}`);
  }

  getEmployeeSalaryHistory(employeeId: number): Observable<EmployeeSalaryDto[]> {
    return this.http.get<EmployeeSalaryDto[]>(`${this.baseUrl}/salaries/employee/${employeeId}/history`);
  }

  setSalary(dto: SetEmployeeSalaryDto): Observable<EmployeeSalaryDto> {
    return this.http.post<EmployeeSalaryDto>(`${this.baseUrl}/salaries`, dto);
  }

  getMySalary(): Observable<EmployeeSalaryDto> {
    return this.http.get<EmployeeSalaryDto>(`${this.baseUrl}/salaries/my`);
  }

  // ---------- Payroll runs ----------

  getRuns(): Observable<PayrollRunDto[]> {
    return this.http.get<PayrollRunDto[]>(`${this.baseUrl}/runs`);
  }

  getRun(id: number): Observable<PayrollRunDetail> {
    return this.http.get<PayrollRunDetail>(`${this.baseUrl}/runs/${id}`);
  }

  createRun(dto: CreatePayrollRunDto): Observable<PayrollRunDto> {
    return this.http.post<PayrollRunDto>(`${this.baseUrl}/runs`, dto);
  }

  processRun(id: number): Observable<PayrollRunDto> {
    return this.http.post<PayrollRunDto>(`${this.baseUrl}/runs/${id}/process`, {});
  }

  lockRun(id: number): Observable<PayrollRunDto> {
    return this.http.post<PayrollRunDto>(`${this.baseUrl}/runs/${id}/lock`, {});
  }

  markPaid(id: number): Observable<PayrollRunDto> {
    return this.http.post<PayrollRunDto>(`${this.baseUrl}/runs/${id}/mark-paid`, {});
  }

  deleteRun(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/runs/${id}`);
  }

  getBankFile(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/runs/${id}/bank-file`, { responseType: 'blob' });
  }

  // ---------- Payslips ----------

  getPayslipsByRun(runId: number): Observable<PayslipDto[]> {
    return this.http.get<PayslipDto[]>(`${this.baseUrl}/payslips/run/${runId}`);
  }

  getPayslip(id: number): Observable<PayslipDto> {
    return this.http.get<PayslipDto>(`${this.baseUrl}/payslips/${id}`);
  }

  getMyPayslips(): Observable<PayslipDto[]> {
    return this.http.get<PayslipDto[]>(`${this.baseUrl}/payslips/my`);
  }

  getMyPayslip(id: number): Observable<PayslipDto> {
    return this.http.get<PayslipDto>(`${this.baseUrl}/payslips/my/${id}`);
  }
}
