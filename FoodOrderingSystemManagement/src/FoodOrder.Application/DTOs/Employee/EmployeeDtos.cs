namespace FoodOrder.Application.DTOs.Employee;

// ── User Edit/Delete ─────────────────────────────────────────────────────────

public class UpdateUserDto
{
    public string  Name    { get; set; } = string.Empty;
    public string  Email   { get; set; } = string.Empty;
    public int     RoleId  { get; set; }
    public string? Phone   { get; set; }
}

// ── Attendance ───────────────────────────────────────────────────────────────

public class AttendanceDto
{
    public int      Id       { get; set; }
    public int      UserId   { get; set; }
    public string   UserName { get; set; } = string.Empty;
    public DateTime Date     { get; set; }
    public string?  CheckIn  { get; set; }
    public string?  CheckOut { get; set; }
    public string   Status   { get; set; } = string.Empty;
    public string?  Notes    { get; set; }
}

public class UpsertAttendanceRequest
{
    public int      UserId   { get; set; }
    public DateTime Date     { get; set; }
    public string?  CheckIn  { get; set; }  // "HH:mm"
    public string?  CheckOut { get; set; }
    public string   Status   { get; set; } = "Present";
    public string?  Notes    { get; set; }
}

// ── Salary ───────────────────────────────────────────────────────────────────

public class EmployeeSalaryDto
{
    public int     Id          { get; set; }
    public int     UserId      { get; set; }
    public string  UserName    { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowances  { get; set; }
    public string  PayPeriod   { get; set; } = string.Empty;
    public decimal TotalGross  => BasicSalary + Allowances;
}

public class UpdateSalaryConfigRequest
{
    public decimal BasicSalary { get; set; }
    public decimal Allowances  { get; set; }
    public string  PayPeriod   { get; set; } = "Monthly";
}

public class SalaryPaymentDto
{
    public int      Id         { get; set; }
    public int      UserId     { get; set; }
    public string   UserName   { get; set; } = string.Empty;
    public int      Month      { get; set; }
    public int      Year       { get; set; }
    public decimal  GrossPay   { get; set; }
    public decimal  Deductions { get; set; }
    public decimal  NetPay     { get; set; }
    public string   Status     { get; set; } = string.Empty;
    public DateTime? PaidDate  { get; set; }
    public string?  Notes      { get; set; }
}

public class CreateSalaryPaymentRequest
{
    public int     UserId     { get; set; }
    public int     Month      { get; set; }
    public int     Year       { get; set; }
    public decimal GrossPay   { get; set; }
    public decimal Deductions { get; set; }
    public string? Notes      { get; set; }
}

// ── Shifts ───────────────────────────────────────────────────────────────────

public class ShiftDefinitionDto
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime   { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}

public class CreateShiftDefinitionRequest
{
    public string  Name      { get; set; } = string.Empty;
    public string  StartTime { get; set; } = string.Empty;  // "HH:mm"
    public string  EndTime   { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}

public class ShiftAssignmentDto
{
    public int      Id        { get; set; }
    public int      UserId    { get; set; }
    public string   UserName  { get; set; } = string.Empty;
    public int      ShiftId   { get; set; }
    public string   ShiftName { get; set; } = string.Empty;
    public string   ShiftStart { get; set; } = string.Empty;
    public string   ShiftEnd   { get; set; } = string.Empty;
    public string?  ShiftColor { get; set; }
    public DateTime Date       { get; set; }
}

public class CreateShiftAssignmentRequest
{
    public int      UserId  { get; set; }
    public int      ShiftId { get; set; }
    public DateTime Date    { get; set; }
}

// ── Performance ──────────────────────────────────────────────────────────────

public class PerformanceReviewDto
{
    public int      Id             { get; set; }
    public int      UserId         { get; set; }
    public string   UserName       { get; set; } = string.Empty;
    public int      ReviewedById   { get; set; }
    public string   ReviewedByName { get; set; } = string.Empty;
    public DateTime ReviewDate     { get; set; }
    public int      Rating         { get; set; }
    public string   Category       { get; set; } = string.Empty;
    public string   Comments       { get; set; } = string.Empty;
}

public class CreatePerformanceReviewRequest
{
    public int      UserId     { get; set; }
    public DateTime ReviewDate { get; set; }
    public int      Rating     { get; set; }
    public string   Category   { get; set; } = "General";
    public string   Comments   { get; set; } = string.Empty;
}
