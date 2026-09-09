namespace NovaERP.Domain.Entities;

/// <summary>An employee's request for time off against a LeaveType. DaysRequested is entered
/// directly by the requester rather than computed from StartDate/EndDate — accurately
/// excluding weekends/holidays is out of scope for this pass. No "approved by" field: Phase
/// 1's generic AuditLog already captures who performed which action.</summary>
public class LeaveRequest : BaseEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId     { get; set; }
    public int LeaveTypeId    { get; set; }

    public DateTime StartDate     { get; set; }
    public DateTime EndDate       { get; set; }
    public decimal  DaysRequested { get; set; }
    public string?  Reason        { get; set; }
    public string   Status        { get; set; } = "Pending"; // Pending | Approved | Rejected | Cancelled

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
    public LeaveType    LeaveType    { get; set; } = null!;
}
