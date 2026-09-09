namespace NovaERP.Application.DTOs;

public class ProjectDto
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public int     ManagerId      { get; set; }
    public string  ManagerName    { get; set; } = string.Empty;

    public DateTime  StartDate { get; set; }
    public DateTime? EndDate   { get; set; }
    public decimal?  Budget    { get; set; }
    public string    Status    { get; set; } = string.Empty;

    /// <summary>Both computed on read, never stored.</summary>
    public int TaskCount          { get; set; }
    public int CompletedTaskCount { get; set; }
}

public class CreateProjectDto
{
    public string  Code        { get; set; } = string.Empty;
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int     ManagerId   { get; set; }

    public DateTime  StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EndDate   { get; set; }
    public decimal?  Budget    { get; set; }
}

/// <summary>Code is immutable after creation — same convention as Product.Sku/Warehouse.Code.</summary>
public class UpdateProjectDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int     ManagerId   { get; set; }

    public DateTime  StartDate { get; set; }
    public DateTime? EndDate   { get; set; }
    public decimal?  Budget    { get; set; }
    public string    Status    { get; set; } = string.Empty;
}

public class ProjectTaskDto
{
    public int     Id             { get; set; }
    public int     ProjectId      { get; set; }
    public string  ProjectName    { get; set; } = string.Empty;
    public int?    AssignedToId   { get; set; }
    public string? AssignedToName { get; set; }

    public string  Title       { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  Priority    { get; set; } = string.Empty;
    public string  Status      { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate   { get; set; }
}

public class CreateProjectTaskDto
{
    public int  ProjectId    { get; set; }
    public int? AssignedToId { get; set; }

    public string  Title       { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  Priority    { get; set; } = "Medium";
    public string  Status      { get; set; } = "ToDo";

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate   { get; set; }
}

/// <summary>ProjectId is immutable after creation — moving a task to a different project is a
/// bigger operation this MVP doesn't model, same reasoning as Employee.DepartmentId.</summary>
public class UpdateProjectTaskDto
{
    public int? AssignedToId { get; set; }

    public string  Title       { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  Priority    { get; set; } = string.Empty;
    public string  Status      { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate   { get; set; }
}
