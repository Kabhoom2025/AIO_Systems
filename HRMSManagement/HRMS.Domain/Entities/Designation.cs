namespace HRMS.Domain.Entities;

public class Designation : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Title          { get; set; } = string.Empty;
    public string  Code           { get; set; } = string.Empty;
    public int?    JobGradeId     { get; set; }
    public string? Description    { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public JobGrade?    JobGrade     { get; set; }
}

public class JobGrade : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty; // e.g. G1, G2 …
    public int     Level          { get; set; }                 // 1 = lowest
    public decimal MinAnnualSalary { get; set; }
    public decimal MaxAnnualSalary { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}
