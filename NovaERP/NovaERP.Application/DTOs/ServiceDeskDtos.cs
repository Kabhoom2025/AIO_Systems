namespace NovaERP.Application.DTOs;

public class TicketCategoryDto
{
    public int    Id       { get; set; }
    public string Name     { get; set; } = string.Empty;
    public string Code     { get; set; } = string.Empty;
    public bool   IsActive { get; set; }
}

public class CreateTicketCategoryDto
{
    public string Name     { get; set; } = string.Empty;
    public string Code     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as AssetCategory.Code.</summary>
public class UpdateTicketCategoryDto
{
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

public class ServiceTicketDto
{
    public int    Id             { get; set; }
    public string TicketNumber   { get; set; } = string.Empty;
    public string Subject        { get; set; } = string.Empty;
    public string Description    { get; set; } = string.Empty;
    public int    CategoryId     { get; set; }
    public string CategoryName   { get; set; } = string.Empty;
    public int    RequesterId    { get; set; }
    public string RequesterName  { get; set; } = string.Empty;
    public int?    AssignedToId   { get; set; }
    public string? AssignedToName { get; set; }

    public string  Priority        { get; set; } = string.Empty;
    public string  Status          { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedDate  { get; set; }
    public DateTime? ClosedDate    { get; set; }
}

public class CreateServiceTicketDto
{
    public string Subject     { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    CategoryId  { get; set; }
    public int    RequesterId { get; set; }
    public string Priority    { get; set; } = "Medium";
}

public class UpdateServiceTicketDto
{
    public string Subject     { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    CategoryId  { get; set; }
    public string Priority    { get; set; } = string.Empty;
}

public class AssignTicketDto
{
    public int EmployeeId { get; set; }
}

public class ResolveTicketDto
{
    public string ResolutionNotes { get; set; } = string.Empty;
}
