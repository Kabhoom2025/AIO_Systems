namespace HRMS.Domain.Entities;

public class LeaveType : BaseEntity
{
    public int     OrganizationId  { get; set; }
    public string  Name            { get; set; } = string.Empty; // Casual, Sick, Earned …
    public string  Code            { get; set; } = string.Empty;
    public bool    IsPaid          { get; set; } = true;
    public decimal AnnualQuota     { get; set; }
    public decimal MaxCarryForward { get; set; }
    public bool    RequiresApproval { get; set; } = true;
    public string? Color           { get; set; } // hex color for calendars
    public bool    IsActive        { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}

public class LeaveBalance : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int     EmployeeId     { get; set; }
    public int     LeaveTypeId    { get; set; }
    public int     Year           { get; set; }
    public decimal Allocated      { get; set; }
    public decimal Used           { get; set; }
    public decimal CarriedForward { get; set; }

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
    public LeaveType    LeaveType    { get; set; } = null!;

    public decimal Available => Allocated + CarriedForward - Used;
}

public class LeaveRequest : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public int      EmployeeId       { get; set; }
    public int      LeaveTypeId      { get; set; }
    public DateOnly StartDate        { get; set; }
    public DateOnly EndDate          { get; set; }
    public decimal  Days             { get; set; }
    public bool     IsHalfDay        { get; set; }
    public string   Reason           { get; set; } = string.Empty;
    public string   Status           { get; set; } = "Pending"; // Pending | Approved | Rejected | Cancelled
    public int?     ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt      { get; set; }
    public string?  ReviewNotes      { get; set; }

    public Organization Organization   { get; set; } = null!;
    public Employee     Employee       { get; set; } = null!;
    public LeaveType    LeaveType      { get; set; } = null!;
    public User?        ReviewedByUser { get; set; }
}
