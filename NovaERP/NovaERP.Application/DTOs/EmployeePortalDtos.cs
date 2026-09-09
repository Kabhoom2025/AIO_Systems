namespace NovaERP.Application.DTOs;

/// <summary>No EmployeeId field — unlike the admin CreateLeaveRequestDto, a portal user can
/// only ever file a leave request for themselves; the service resolves EmployeeId from the
/// caller's own linked Employee record.</summary>
public class CreateMyLeaveRequestDto
{
    public int LeaveTypeId { get; set; }

    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal  DaysRequested { get; set; }
    public string?  Reason        { get; set; }
}

/// <summary>A PayRunLine plus the period info from its parent PayRun — a payslip needs to
/// say which period it's for, which PayRunLineDto alone doesn't carry.</summary>
public class MyPayslipDto
{
    public int    PayRunId     { get; set; }
    public string RunNumber    { get; set; } = string.Empty;
    public int    PeriodMonth  { get; set; }
    public int    PeriodYear   { get; set; }

    public decimal BasicSalary     { get; set; }
    public decimal Hra             { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Deductions      { get; set; }
    public decimal GrossPay        { get; set; }
    public decimal NetPay          { get; set; }
}
