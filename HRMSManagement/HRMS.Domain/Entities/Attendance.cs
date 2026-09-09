namespace HRMS.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public int      EmployeeId       { get; set; }
    public DateOnly Date             { get; set; }
    public DateTime? CheckInAt       { get; set; }
    public DateTime? CheckOutAt      { get; set; }
    public string   Status           { get; set; } = "Present"; // Present | Absent | HalfDay | Leave | Holiday | WeekOff
    public int      WorkedMinutes    { get; set; }
    public int      OvertimeMinutes  { get; set; }
    public int      LateMinutes      { get; set; }
    public int      EarlyExitMinutes { get; set; }
    public string   Source           { get; set; } = "Web";     // Web | Mobile | Biometric
    public string?  Location         { get; set; }              // GPS / device location if provided
    public string?  Notes            { get; set; }

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
}

/// <summary>Request to correct a day's attendance (missed punch etc.), approved by a manager.</summary>
public class AttendanceRegularization : BaseEntity
{
    public int      OrganizationId     { get; set; }
    public int      EmployeeId         { get; set; }
    public DateOnly Date               { get; set; }
    public DateTime? RequestedCheckIn  { get; set; }
    public DateTime? RequestedCheckOut { get; set; }
    public string   Reason             { get; set; } = string.Empty;
    public string   Status             { get; set; } = "Pending"; // Pending | Approved | Rejected
    public int?     ReviewedByUserId   { get; set; }
    public DateTime? ReviewedAt        { get; set; }
    public string?  ReviewNotes        { get; set; }

    public Organization Organization   { get; set; } = null!;
    public Employee     Employee       { get; set; } = null!;
    public User?        ReviewedByUser { get; set; }
}
