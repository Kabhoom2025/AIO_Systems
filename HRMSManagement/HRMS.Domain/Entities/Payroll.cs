namespace HRMS.Domain.Entities;

public class SalaryComponent : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty; // Basic, HRA, PF …
    public string  Code           { get; set; } = string.Empty;
    public string  Type           { get; set; } = "Earning";    // Earning | Deduction
    public string  CalcType       { get; set; } = "Fixed";      // Fixed | PercentOfBasic
    public decimal DefaultValue   { get; set; }                 // amount or percent depending on CalcType
    public bool    IsTaxable      { get; set; } = true;
    public bool    IsStatutory    { get; set; }                 // PF / ESI / Professional Tax
    public int     DisplayOrder   { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}

/// <summary>An employee's salary structure effective from a date.</summary>
public class EmployeeSalary : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      EmployeeId     { get; set; }
    public DateOnly EffectiveFrom  { get; set; }
    public decimal  AnnualCtc      { get; set; }
    public decimal  MonthlyGross   { get; set; }
    public string   Currency       { get; set; } = "INR";
    public string?  Notes          { get; set; }

    public Organization Organization { get; set; } = null!;
    public Employee     Employee     { get; set; } = null!;
    public ICollection<EmployeeSalaryItem> Items { get; set; } = new List<EmployeeSalaryItem>();
}

public class EmployeeSalaryItem : BaseEntity
{
    public int     EmployeeSalaryId  { get; set; }
    public int     SalaryComponentId { get; set; }
    public decimal MonthlyAmount     { get; set; }

    public EmployeeSalary  EmployeeSalary  { get; set; } = null!;
    public SalaryComponent SalaryComponent { get; set; } = null!;
}

public class PayrollRun : BaseEntity
{
    public int      OrganizationId    { get; set; }
    public int      Year              { get; set; }
    public int      Month             { get; set; } // 1-12
    public string   Status            { get; set; } = "Draft"; // Draft | Processing | Completed | Locked | Paid
    public DateTime? ProcessedAt      { get; set; }
    public int?     ProcessedByUserId { get; set; }
    public decimal  TotalGross        { get; set; }
    public decimal  TotalDeductions   { get; set; }
    public decimal  TotalNet          { get; set; }
    public string?  Notes             { get; set; }

    public Organization Organization    { get; set; } = null!;
    public User?        ProcessedByUser { get; set; }
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}

public class Payslip : BaseEntity
{
    public int     PayrollRunId    { get; set; }
    public int     EmployeeId      { get; set; }
    public decimal GrossEarnings   { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay          { get; set; }
    public decimal WorkingDays     { get; set; }
    public decimal PresentDays     { get; set; }
    public decimal PaidLeaveDays   { get; set; }
    public decimal LopDays         { get; set; } // loss of pay
    public string  Status          { get; set; } = "Generated"; // Generated | Published | Paid

    public PayrollRun PayrollRun { get; set; } = null!;
    public Employee   Employee   { get; set; } = null!;
    public ICollection<PayslipItem> Items { get; set; } = new List<PayslipItem>();
}

public class PayslipItem : BaseEntity
{
    public int     PayslipId     { get; set; }
    public string  ComponentName { get; set; } = string.Empty;
    public string  Type          { get; set; } = "Earning"; // Earning | Deduction
    public decimal Amount        { get; set; }

    public Payslip Payslip { get; set; } = null!;
}
