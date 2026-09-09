namespace HRMS.Application.DTOs;

public class HeadcountReportRow
{
    public string DepartmentName { get; set; } = string.Empty;
    public string BranchName     { get; set; } = string.Empty;
    public int    ActiveCount    { get; set; }
    public int    OnNoticeCount  { get; set; }
    public int    ExitedCount    { get; set; }
}

public class AttendanceMonthlyReportRow
{
    public string  EmployeeCode  { get; set; } = string.Empty;
    public string  EmployeeName  { get; set; } = string.Empty;
    public decimal PresentDays   { get; set; }
    public decimal LeaveDays     { get; set; }
    public int     LateCount     { get; set; }
    public decimal OvertimeHours { get; set; }
}

public class LeaveReportRow
{
    public string  EmployeeCode  { get; set; } = string.Empty;
    public string  EmployeeName  { get; set; } = string.Empty;
    public string  LeaveTypeName { get; set; } = string.Empty;
    public decimal Allocated     { get; set; }
    public decimal Used          { get; set; }
    public decimal Available     { get; set; }
}

public class PayrollSummaryReportRow
{
    public int     Year            { get; set; }
    public int     Month           { get; set; }
    public string  Status          { get; set; } = string.Empty;
    public int     EmployeeCount   { get; set; }
    public decimal TotalGross      { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet        { get; set; }
}

public class RecruitmentPipelineReportRow
{
    public string DepartmentName { get; set; } = string.Empty;
    public string Title          { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
    public int    Vacancies      { get; set; }
    public int    TotalCandidates{ get; set; }
    public int    Applied        { get; set; }
    public int    Screening      { get; set; }
    public int    Interview      { get; set; }
    public int    Offered        { get; set; }
    public int    Hired          { get; set; }
    public int    Rejected       { get; set; }
}

public class AssetReportRow
{
    public string   AssetTag      { get; set; } = string.Empty;
    public string   Name          { get; set; } = string.Empty;
    public string   Category      { get; set; } = string.Empty;
    public string   Status        { get; set; } = string.Empty;
    public string   Condition     { get; set; } = string.Empty;
    public string   CurrentHolder { get; set; } = string.Empty;
    public decimal? PurchaseCost  { get; set; }
    public DateOnly? WarrantyUntil{ get; set; }
}
