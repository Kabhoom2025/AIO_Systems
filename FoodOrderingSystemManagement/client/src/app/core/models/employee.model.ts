export interface AttendanceRecord {
  id: number;
  userId: number;
  userName: string;
  date: string;
  checkIn?: string;
  checkOut?: string;
  status: 'Present' | 'Absent' | 'Late' | 'HalfDay' | 'Leave';
  notes?: string;
}

export interface UpsertAttendanceRequest {
  userId: number;
  date: string;
  checkIn?: string;
  checkOut?: string;
  status: string;
  notes?: string;
}

export interface EmployeeSalaryConfig {
  id: number;
  userId: number;
  userName: string;
  basicSalary: number;
  allowances: number;
  payPeriod: string;
  totalGross: number;
}

export interface UpdateSalaryConfigRequest {
  basicSalary: number;
  allowances: number;
  payPeriod: string;
}

export interface SalaryPayment {
  id: number;
  userId: number;
  userName: string;
  month: number;
  year: number;
  grossPay: number;
  deductions: number;
  netPay: number;
  status: 'Pending' | 'Paid';
  paidDate?: string;
  notes?: string;
}

export interface CreateSalaryPaymentRequest {
  userId: number;
  month: number;
  year: number;
  grossPay: number;
  deductions: number;
  notes?: string;
}

export interface ShiftDefinition {
  id: number;
  name: string;
  startTime: string;
  endTime: string;
  colorCode?: string;
}

export interface CreateShiftDefinitionRequest {
  name: string;
  startTime: string;
  endTime: string;
  colorCode?: string;
}

export interface ShiftAssignment {
  id: number;
  userId: number;
  userName: string;
  shiftId: number;
  shiftName: string;
  shiftStart: string;
  shiftEnd: string;
  shiftColor?: string;
  date: string;
}

export interface CreateShiftAssignmentRequest {
  userId: number;
  shiftId: number;
  date: string;
}

export interface PerformanceReview {
  id: number;
  userId: number;
  userName: string;
  reviewedById: number;
  reviewedByName: string;
  reviewDate: string;
  rating: number;
  category: string;
  comments: string;
}

export interface CreatePerformanceReviewRequest {
  userId: number;
  reviewDate: string;
  rating: number;
  category: string;
  comments: string;
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  roleId: number;
  phone?: string;
}
