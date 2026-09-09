namespace HRMS.Application.DTOs;

public class AttendanceRecordDto
{
    public int       Id               { get; set; }
    public int       EmployeeId       { get; set; }
    public DateOnly  Date             { get; set; }
    public DateTime? CheckInAt        { get; set; }
    public DateTime? CheckOutAt       { get; set; }
    public string    Status           { get; set; } = "Present";
    public int       WorkedMinutes    { get; set; }
    public int       OvertimeMinutes  { get; set; }
    public int       LateMinutes      { get; set; }
    public int       EarlyExitMinutes { get; set; }
    public string    Source           { get; set; } = "Web";
    public string?   Location         { get; set; }
    public string?   Notes            { get; set; }
}

public class AttendanceDayDto
{
    public int       EmployeeId      { get; set; }
    public string    EmployeeCode    { get; set; } = string.Empty;
    public string    EmployeeName    { get; set; } = string.Empty;
    public string    DepartmentName  { get; set; } = string.Empty;
    public DateTime? CheckInAt       { get; set; }
    public DateTime? CheckOutAt      { get; set; }
    public string    Status          { get; set; } = string.Empty;
    public int       WorkedMinutes   { get; set; }
    public int       LateMinutes     { get; set; }
    public int       OvertimeMinutes { get; set; }
}

public class AttendanceSummaryDto
{
    public int TotalEmployees { get; set; }
    public int Present        { get; set; }
    public int Absent         { get; set; }
    public int OnLeave        { get; set; }
    public int Late           { get; set; }
}

public class CheckInDto
{
    public string? Location { get; set; }
    public string  Source   { get; set; } = "Web";
}

public class RegularizationDto
{
    public int       Id                { get; set; }
    public int       EmployeeId        { get; set; }
    public string    EmployeeName      { get; set; } = string.Empty;
    public DateOnly  Date              { get; set; }
    public DateTime? RequestedCheckIn  { get; set; }
    public DateTime? RequestedCheckOut { get; set; }
    public string    Reason            { get; set; } = string.Empty;
    public string    Status            { get; set; } = "Pending";
    public int?      ReviewedByUserId  { get; set; }
    public DateTime? ReviewedAt        { get; set; }
    public string?   ReviewNotes       { get; set; }
    public DateTime  CreatedDate       { get; set; }
}

public class CreateRegularizationDto
{
    public DateOnly  Date              { get; set; }
    public DateTime? RequestedCheckIn  { get; set; }
    public DateTime? RequestedCheckOut { get; set; }
    public string    Reason            { get; set; } = string.Empty;
}

public class ReviewDto
{
    public string? Notes { get; set; }
}
