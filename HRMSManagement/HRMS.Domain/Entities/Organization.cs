namespace HRMS.Domain.Entities;

public class Organization : BaseEntity
{
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? LegalName   { get; set; }
    public string? Address     { get; set; }
    public string? Phone       { get; set; }
    public string? Email       { get; set; }
    public string? Website     { get; set; }
    public string? TaxNumber   { get; set; }
    public string  Timezone    { get; set; } = "Asia/Kolkata";
    public string  Currency    { get; set; } = "INR";
    public string? LogoUrl     { get; set; }
    public bool    IsActive    { get; set; } = true;

    public ICollection<Branch>     Branches     { get; set; } = new List<Branch>();
    public ICollection<Department> Departments  { get; set; } = new List<Department>();
    public ICollection<Employee>   Employees    { get; set; } = new List<Employee>();
}
