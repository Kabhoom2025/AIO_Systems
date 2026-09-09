namespace HRMS.Application.DTOs;

public class SalaryComponentDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string  Type         { get; set; } = "Earning";
    public string  CalcType     { get; set; } = "Fixed";
    public decimal DefaultValue { get; set; }
    public bool    IsTaxable    { get; set; }
    public bool    IsStatutory  { get; set; }
    public int     DisplayOrder { get; set; }
    public bool    IsActive     { get; set; }
}

public class CreateSalaryComponentDto
{
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string  Type         { get; set; } = "Earning";
    public string  CalcType     { get; set; } = "Fixed";
    public decimal DefaultValue { get; set; }
    public bool    IsTaxable    { get; set; } = true;
    public bool    IsStatutory  { get; set; }
    public int     DisplayOrder { get; set; }
}

public class UpdateSalaryComponentDto
{
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string  Type         { get; set; } = "Earning";
    public string  CalcType     { get; set; } = "Fixed";
    public decimal DefaultValue { get; set; }
    public bool    IsTaxable    { get; set; } = true;
    public bool    IsStatutory  { get; set; }
    public int     DisplayOrder { get; set; }
    public bool    IsActive     { get; set; } = true;
}

public class EmployeeSalaryItemDto
{
    public int     ComponentId   { get; set; }
    public string  ComponentName { get; set; } = string.Empty;
    public string  Type          { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; }
}

public class EmployeeSalaryDto
{
    public int      Id            { get; set; }
    public int      EmployeeId    { get; set; }
    public string   EmployeeName  { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public decimal  AnnualCtc     { get; set; }
    public decimal  MonthlyGross  { get; set; }
    public string   Currency      { get; set; } = "INR";
    public string?  Notes         { get; set; }
    public List<EmployeeSalaryItemDto> Items { get; set; } = new();
}

public class SetEmployeeSalaryItemDto
{
    public int     SalaryComponentId { get; set; }
    public decimal MonthlyAmount     { get; set; }
}

public class SetEmployeeSalaryDto
{
    public int      EmployeeId    { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public decimal  AnnualCtc     { get; set; }
    public string   Currency      { get; set; } = "INR";
    public string?  Notes         { get; set; }
    public List<SetEmployeeSalaryItemDto> Items { get; set; } = new();
}

public class PayrollRunDto
{
    public int       Id              { get; set; }
    public int       Year            { get; set; }
    public int       Month           { get; set; }
    public string    Status          { get; set; } = "Draft";
    public DateTime? ProcessedAt     { get; set; }
    public string?   ProcessedByName { get; set; }
    public decimal   TotalGross      { get; set; }
    public decimal   TotalDeductions { get; set; }
    public decimal   TotalNet        { get; set; }
    public int       PayslipCount    { get; set; }
    public string?   Notes           { get; set; }
}

public class CreatePayrollRunDto
{
    public int     Year  { get; set; }
    public int     Month { get; set; }
    public string? Notes { get; set; }
}

public class PayslipItemDto
{
    public string  ComponentName { get; set; } = string.Empty;
    public string  Type          { get; set; } = "Earning";
    public decimal Amount        { get; set; }
}

public class PayslipDto
{
    public int     Id               { get; set; }
    public int     EmployeeId       { get; set; }
    public string  EmployeeCode     { get; set; } = string.Empty;
    public string  EmployeeName     { get; set; } = string.Empty;
    public string  DepartmentName   { get; set; } = string.Empty;
    public string  DesignationTitle { get; set; } = string.Empty;
    public int     Year             { get; set; }
    public int     Month            { get; set; }
    public decimal GrossEarnings    { get; set; }
    public decimal TotalDeductions  { get; set; }
    public decimal NetPay           { get; set; }
    public decimal WorkingDays      { get; set; }
    public decimal PresentDays      { get; set; }
    public decimal PaidLeaveDays    { get; set; }
    public decimal LopDays          { get; set; }
    public string  Status           { get; set; } = "Generated";
    public List<PayslipItemDto> Items { get; set; } = new();
}
