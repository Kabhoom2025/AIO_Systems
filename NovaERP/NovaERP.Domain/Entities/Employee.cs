namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped HR employee record — distinct from `User` (system login/permissions).
/// Not every employee has a `User` account in this MVP; `UserId` is nullable. Branch is
/// derivable from `Department.BranchId` on read, so no redundant BranchId FK is stored here.
/// Compensation (salary/CTC) is deliberately deferred to the future Payroll module.</summary>
public class Employee : BaseEntity
{
    public int    OrganizationId     { get; set; }
    public int    DepartmentId       { get; set; }
    public int?   UserId             { get; set; }
    public int?   ReportingManagerId { get; set; }

    public string  EmployeeCode  { get; set; } = string.Empty;
    public string  FirstName     { get; set; } = string.Empty;
    public string  LastName      { get; set; } = string.Empty;
    public string  Email         { get; set; } = string.Empty;
    public string? Phone         { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender        { get; set; }

    public string   JobTitle        { get; set; } = string.Empty;
    public string   EmploymentType  { get; set; } = string.Empty; // Full-Time | Part-Time | Contract | Intern
    public DateTime DateOfJoining   { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string   Status          { get; set; } = "Active"; // Active | OnLeave | Terminated | Resigned

    public Organization Organization      { get; set; } = null!;
    public Department   Department        { get; set; } = null!;
    public User?         User             { get; set; }
    public Employee?     ReportingManager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
