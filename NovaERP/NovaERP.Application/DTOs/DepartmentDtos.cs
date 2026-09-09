namespace NovaERP.Application.DTOs;

public class DepartmentDto
{
    public int     Id          { get; set; }
    public int?    BranchId    { get; set; }
    public string? BranchName  { get; set; }
    public int?    ParentId    { get; set; }
    public string? ParentName  { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CostCenter  { get; set; }
    public bool    IsActive    { get; set; }
}

public class CreateDepartmentDto
{
    public int?    BranchId    { get; set; } // null = org-wide department
    public int?    ParentId    { get; set; } // null = top-level
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CostCenter  { get; set; }
}

public class UpdateDepartmentDto
{
    public int?    BranchId    { get; set; }
    public int?    ParentId    { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CostCenter  { get; set; }
    public bool    IsActive    { get; set; }
}
