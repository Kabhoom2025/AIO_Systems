namespace HRMS.Application.DTOs;

public class PerformanceGoalDto
{
    public int      Id              { get; set; }
    public int      EmployeeId      { get; set; }
    public string   EmployeeName    { get; set; } = string.Empty;
    public string   Title           { get; set; } = string.Empty;
    public string?  Description     { get; set; }
    public string?  Metric          { get; set; }
    public decimal  Weight          { get; set; }
    public DateOnly StartDate       { get; set; }
    public DateOnly DueDate         { get; set; }
    public int      ProgressPercent { get; set; }
    public string   Status          { get; set; } = string.Empty;
}

public class CreatePerformanceGoalDto
{
    public int      EmployeeId  { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public string?  Metric      { get; set; }
    public decimal  Weight      { get; set; } = 100;
    public DateOnly StartDate   { get; set; }
    public DateOnly DueDate     { get; set; }
}

public class UpdatePerformanceGoalDto
{
    public string   Title       { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public string?  Metric      { get; set; }
    public decimal  Weight      { get; set; } = 100;
    public DateOnly StartDate   { get; set; }
    public DateOnly DueDate     { get; set; }
    public string   Status      { get; set; } = "NotStarted"; // NotStarted | InProgress | Completed | Cancelled
}

public class UpdateGoalProgressDto
{
    public int     ProgressPercent { get; set; }
    public string? Status          { get; set; } // NotStarted | InProgress | Completed | Cancelled
}

public class PerformanceReviewDto
{
    public int      Id              { get; set; }
    public int      EmployeeId      { get; set; }
    public string   EmployeeName    { get; set; } = string.Empty;
    public int?     ReviewerUserId  { get; set; }
    public string?  ReviewerName    { get; set; }
    public string   Period          { get; set; } = string.Empty;
    public decimal? SelfRating      { get; set; }
    public string?  SelfComments    { get; set; }
    public decimal? ManagerRating   { get; set; }
    public string?  ManagerComments { get; set; }
    public decimal? FinalRating     { get; set; }
    public string   Status          { get; set; } = string.Empty;
    public string   Recommendation  { get; set; } = string.Empty;
    public DateTime? CompletedAt    { get; set; }
}

public class CreatePerformanceReviewDto
{
    public int    EmployeeId     { get; set; }
    public string Period         { get; set; } = string.Empty;
    public int?   ReviewerUserId { get; set; }
}

public class SelfReviewDto
{
    public decimal SelfRating   { get; set; } // 1-5
    public string  SelfComments { get; set; } = string.Empty;
}

public class ManagerReviewDto
{
    public decimal ManagerRating   { get; set; } // 1-5
    public string  ManagerComments { get; set; } = string.Empty;
    public string  Recommendation  { get; set; } = "None"; // None | Promotion | Increment | PIP
}
