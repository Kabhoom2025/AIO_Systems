namespace HRMS.Application.DTOs;

public class LeaveTypeDto
{
    public int     Id               { get; set; }
    public string  Name             { get; set; } = string.Empty;
    public string  Code             { get; set; } = string.Empty;
    public bool    IsPaid           { get; set; }
    public decimal AnnualQuota      { get; set; }
    public decimal MaxCarryForward  { get; set; }
    public bool    RequiresApproval { get; set; }
    public string? Color            { get; set; }
    public bool    IsActive         { get; set; }
}

public class CreateLeaveTypeDto
{
    public string  Name             { get; set; } = string.Empty;
    public string  Code             { get; set; } = string.Empty;
    public bool    IsPaid           { get; set; } = true;
    public decimal AnnualQuota      { get; set; }
    public decimal MaxCarryForward  { get; set; }
    public bool    RequiresApproval { get; set; } = true;
    public string? Color            { get; set; }
}

public class UpdateLeaveTypeDto
{
    public string  Name             { get; set; } = string.Empty;
    public string  Code             { get; set; } = string.Empty;
    public bool    IsPaid           { get; set; } = true;
    public decimal AnnualQuota      { get; set; }
    public decimal MaxCarryForward  { get; set; }
    public bool    RequiresApproval { get; set; } = true;
    public string? Color            { get; set; }
    public bool    IsActive         { get; set; } = true;
}

public class LeaveBalanceDto
{
    public int     Id             { get; set; }
    public int     EmployeeId     { get; set; }
    public int     LeaveTypeId    { get; set; }
    public string  LeaveTypeName  { get; set; } = string.Empty;
    public int     Year           { get; set; }
    public decimal Allocated      { get; set; }
    public decimal Used           { get; set; }
    public decimal CarriedForward { get; set; }
    public decimal Available      { get; set; }
}

public class LeaveRequestDto
{
    public int       Id               { get; set; }
    public int       EmployeeId       { get; set; }
    public string    EmployeeName     { get; set; } = string.Empty;
    public int       LeaveTypeId      { get; set; }
    public string    LeaveTypeName    { get; set; } = string.Empty;
    public DateOnly  StartDate        { get; set; }
    public DateOnly  EndDate          { get; set; }
    public decimal   Days             { get; set; }
    public bool      IsHalfDay        { get; set; }
    public string    Reason           { get; set; } = string.Empty;
    public string    Status           { get; set; } = "Pending";
    public int?      ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt       { get; set; }
    public string?   ReviewNotes      { get; set; }
    public DateTime  CreatedDate      { get; set; }
}

public class CreateLeaveRequestDto
{
    public int      LeaveTypeId { get; set; }
    public DateOnly StartDate   { get; set; }
    public DateOnly EndDate     { get; set; }
    public bool     IsHalfDay   { get; set; }
    public string   Reason      { get; set; } = string.Empty;
}

public class LeaveCalendarDto
{
    public string   EmployeeName  { get; set; } = string.Empty;
    public string   LeaveTypeName { get; set; } = string.Empty;
    public string?  Color         { get; set; }
    public DateOnly StartDate     { get; set; }
    public DateOnly EndDate       { get; set; }
}

public class AllocateBalancesDto
{
    public int Year { get; set; }
}
