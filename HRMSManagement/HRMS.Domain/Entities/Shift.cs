namespace HRMS.Domain.Entities;

public class Shift : BaseEntity
{
    public int      OrganizationId { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string   Code           { get; set; } = string.Empty;
    public TimeOnly StartTime      { get; set; }
    public TimeOnly EndTime        { get; set; }
    public int      BreakMinutes   { get; set; } = 60;
    public int      GraceMinutes   { get; set; } = 10; // late allowance before marking late
    public bool     IsNightShift   { get; set; }
    public string   WeeklyOffDays  { get; set; } = "Saturday,Sunday";
    public bool     IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}

public class Holiday : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int?     BranchId       { get; set; } // null = all branches
    public string   Name           { get; set; } = string.Empty;
    public DateOnly Date           { get; set; }
    public string   Type           { get; set; } = "National"; // National | Regional | Optional
    public string?  Description    { get; set; }

    public Organization Organization { get; set; } = null!;
    public Branch?      Branch       { get; set; }
}
