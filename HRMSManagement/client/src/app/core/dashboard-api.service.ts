import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NameValueDto {
  name: string;
  value: number;
}

export interface DateCountDto {
  date: string;
  present: number;
  onLeave: number;
}

export interface HolidaySummaryDto {
  name: string;
  date: string;
}

export interface RecentJoinerDto {
  employeeCode: string;
  fullName: string;
  designationTitle: string;
  joiningDate: string;
}

export interface BirthdayDto {
  fullName: string;
  dateOfBirth: string;
}

export interface DashboardSummaryDto {
  totalEmployees: number;
  presentToday: number;
  onLeaveToday: number;
  pendingLeaveApprovals: number;
  pendingRegularizations: number;
  pendingExpenseClaims: number;
  openJobOpenings: number;
  openTickets: number;

  upcomingHolidays: HolidaySummaryDto[];
  headcountByDepartment: NameValueDto[];
  employmentTypeSplit: NameValueDto[];
  genderSplit: NameValueDto[];
  attendanceTrend: DateCountDto[];
  recentJoiners: RecentJoinerDto[];
  birthdaysThisMonth: BirthdayDto[];
}

export interface TodayAttendanceDto {
  checkInAt?: string | null;
  checkOutAt?: string | null;
  status: string;
}

export interface LeaveBalanceSummaryDto {
  leaveTypeName: string;
  available: number;
}

export interface PendingRequestsSummaryDto {
  pendingLeaveCount: number;
  pendingRegularizationCount: number;
  pendingExpenseCount: number;
}

export interface LatestPayslipDto {
  year: number;
  month: number;
  netPay: number;
}

export interface MyDashboardDto {
  hasEmployeeProfile: boolean;
  todayAttendance?: TodayAttendanceDto | null;
  leaveBalances: LeaveBalanceSummaryDto[];
  pendingRequests: PendingRequestsSummaryDto;
  goalsInProgress: number;
  latestPayslip?: LatestPayslipDto | null;
  myOpenTickets: number;
  upcomingHolidays: HolidaySummaryDto[];
}

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private baseUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getSummary(): Observable<DashboardSummaryDto> {
    return this.http.get<DashboardSummaryDto>(`${this.baseUrl}/summary`);
  }

  getMyDashboard(): Observable<MyDashboardDto> {
    return this.http.get<MyDashboardDto>(`${this.baseUrl}/my`);
  }
}
