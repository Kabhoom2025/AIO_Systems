namespace HRMS.Application.DTOs;

public class DesignationDto
{
    public int     Id           { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public int?    JobGradeId   { get; set; }
    public string? JobGradeName { get; set; }
    public string? Description  { get; set; }
    public bool    IsActive     { get; set; }
}

public class CreateDesignationDto
{
    public string  Title       { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public int?    JobGradeId  { get; set; }
    public string? Description { get; set; }
}

public class UpdateDesignationDto
{
    public string  Title       { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public int?    JobGradeId  { get; set; }
    public string? Description { get; set; }
    public bool    IsActive    { get; set; }
}

public class JobGradeDto
{
    public int     Id              { get; set; }
    public string  Name            { get; set; } = string.Empty;
    public int     Level           { get; set; }
    public decimal MinAnnualSalary { get; set; }
    public decimal MaxAnnualSalary { get; set; }
    public bool    IsActive        { get; set; }
}

public class CreateJobGradeDto
{
    public string  Name            { get; set; } = string.Empty;
    public int     Level           { get; set; }
    public decimal MinAnnualSalary { get; set; }
    public decimal MaxAnnualSalary { get; set; }
}

public class UpdateJobGradeDto
{
    public string  Name            { get; set; } = string.Empty;
    public int     Level           { get; set; }
    public decimal MinAnnualSalary { get; set; }
    public decimal MaxAnnualSalary { get; set; }
    public bool    IsActive        { get; set; }
}
