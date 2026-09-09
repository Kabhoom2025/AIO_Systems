namespace NovaERP.Domain.Entities;

/// <summary>A snapshot of one employee's compensation at the moment a PayRun was Processed —
/// not a live reference to EmployeeCompensation, which can change later and must not cause a
/// historical pay run to silently drift.</summary>
public class PayRunLine : BaseEntity
{
    public int PayRunId   { get; set; }
    public int EmployeeId { get; set; }

    public decimal BasicSalary     { get; set; }
    public decimal Hra             { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Deductions      { get; set; }
    public decimal GrossPay        { get; set; }
    public decimal NetPay          { get; set; }

    public PayRun   PayRun   { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
