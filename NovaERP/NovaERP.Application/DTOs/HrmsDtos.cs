namespace NovaERP.Application.DTOs;

public class EmployeeDto
{
    public int    Id                     { get; set; }
    public string EmployeeCode           { get; set; } = string.Empty;
    public int    DepartmentId           { get; set; }
    public string DepartmentName         { get; set; } = string.Empty;
    public int?   UserId                 { get; set; }
    public string? UserName              { get; set; }
    public int?   ReportingManagerId     { get; set; }
    public string? ReportingManagerName  { get; set; }

    public string  FirstName     { get; set; } = string.Empty;
    public string  LastName      { get; set; } = string.Empty;
    public string  Email         { get; set; } = string.Empty;
    public string? Phone         { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender        { get; set; }

    public string    JobTitle         { get; set; } = string.Empty;
    public string    EmploymentType   { get; set; } = string.Empty;
    public DateTime  DateOfJoining    { get; set; }
    public DateTime? TerminationDate  { get; set; }
    public string    Status           { get; set; } = string.Empty;
}

public class CreateEmployeeDto
{
    public int  DepartmentId       { get; set; }
    public int? UserId             { get; set; }
    public int? ReportingManagerId { get; set; }

    public string  FirstName     { get; set; } = string.Empty;
    public string  LastName      { get; set; } = string.Empty;
    public string  Email         { get; set; } = string.Empty;
    public string? Phone         { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender        { get; set; }

    public string   JobTitle       { get; set; } = string.Empty;
    public string   EmploymentType { get; set; } = string.Empty;
    public DateTime DateOfJoining  { get; set; } = DateTime.UtcNow.Date;
}

/// <summary>DepartmentId is not editable post-creation in this pass — a department transfer
/// is a bigger operation (approvals, effective-dated history) this MVP doesn't model yet.</summary>
public class UpdateEmployeeDto
{
    public int? UserId             { get; set; }
    public int? ReportingManagerId { get; set; }

    public string  FirstName     { get; set; } = string.Empty;
    public string  LastName      { get; set; } = string.Empty;
    public string  Email         { get; set; } = string.Empty;
    public string? Phone         { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender        { get; set; }

    public string   JobTitle       { get; set; } = string.Empty;
    public string   EmploymentType { get; set; } = string.Empty;
    public DateTime DateOfJoining  { get; set; }
}

public class LeaveTypeDto
{
    public int      Id                 { get; set; }
    public string   Name               { get; set; } = string.Empty;
    public string   Code               { get; set; } = string.Empty;
    public decimal? DefaultDaysPerYear { get; set; }
    public bool     IsActive           { get; set; }
}

public class CreateLeaveTypeDto
{
    public string   Name               { get; set; } = string.Empty;
    public string   Code               { get; set; } = string.Empty;
    public decimal? DefaultDaysPerYear { get; set; }
    public bool     IsActive           { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Product.Sku/Warehouse.Code.</summary>
public class UpdateLeaveTypeDto
{
    public string   Name               { get; set; } = string.Empty;
    public decimal? DefaultDaysPerYear { get; set; }
    public bool     IsActive           { get; set; } = true;
}

public class LeaveRequestDto
{
    public int    Id             { get; set; }
    public int    EmployeeId     { get; set; }
    public string EmployeeName   { get; set; } = string.Empty;
    public int    LeaveTypeId    { get; set; }
    public string LeaveTypeName  { get; set; } = string.Empty;

    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal  DaysRequested { get; set; }
    public string?  Reason        { get; set; }
    public string   Status        { get; set; } = string.Empty;
}

public class CreateLeaveRequestDto
{
    public int EmployeeId  { get; set; }
    public int LeaveTypeId { get; set; }

    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal  DaysRequested { get; set; }
    public string?  Reason        { get; set; }
}

public class UpdateLeaveRequestDto
{
    public int LeaveTypeId { get; set; }

    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal  DaysRequested { get; set; }
    public string?  Reason        { get; set; }
}
