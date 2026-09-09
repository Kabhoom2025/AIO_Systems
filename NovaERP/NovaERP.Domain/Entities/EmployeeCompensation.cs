namespace NovaERP.Domain.Entities;

/// <summary>One active compensation record per Employee (unique per org, no effective-dated
/// history in this pass). Deductions is one lump monthly figure — no statutory PF/ESI/tax-slab
/// component breakdown. GrossPay/NetPay are computed on read, never stored.</summary>
public class EmployeeCompensation : BaseEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId     { get; set; }

    public decimal BasicSalary    { get; set; }
    public decimal Hra            { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Deductions     { get; set; }

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
}
