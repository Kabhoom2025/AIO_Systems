namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped leave-type reference data (e.g. "Annual Leave", "Sick Leave"). No
/// balance/accrual tracking against DefaultDaysPerYear in this pass — informational only.</summary>
public class LeaveType : BaseEntity
{
    public int     OrganizationId    { get; set; }
    public string  Name              { get; set; } = string.Empty;
    public string  Code              { get; set; } = string.Empty;
    public decimal? DefaultDaysPerYear { get; set; }
    public bool    IsActive          { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
