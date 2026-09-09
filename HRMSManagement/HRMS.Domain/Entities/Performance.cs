namespace HRMS.Domain.Entities;

public class PerformanceGoal : BaseEntity
{
    public int      OrganizationId  { get; set; }
    public int      EmployeeId      { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public string?  Description     { get; set; }
    public string?  Metric          { get; set; } // KPI / OKR measure
    public decimal  Weight          { get; set; } = 100; // % weight in appraisal
    public DateOnly StartDate       { get; set; }
    public DateOnly DueDate         { get; set; }
    public int      ProgressPercent { get; set; }
    public string   Status          { get; set; } = "NotStarted"; // NotStarted | InProgress | Completed | Cancelled

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
}

public class PerformanceReview : BaseEntity
{
    public int      OrganizationId  { get; set; }
    public int      EmployeeId      { get; set; }
    public int?     ReviewerUserId  { get; set; }
    public string   Period          { get; set; } = string.Empty; // e.g. FY2026-H1
    public decimal? SelfRating      { get; set; }  // 1-5
    public string?  SelfComments    { get; set; }
    public decimal? ManagerRating   { get; set; }  // 1-5
    public string?  ManagerComments { get; set; }
    public decimal? FinalRating     { get; set; }
    public string   Status          { get; set; } = "Draft"; // Draft | SelfReview | ManagerReview | Completed
    public string   Recommendation  { get; set; } = "None";  // None | Promotion | Increment | PIP
    public DateTime? CompletedAt    { get; set; }

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
    public User?        ReviewerUser { get; set; }
}
