namespace FoodOrder.Application.DTOs.PlatformModule;

public class PlatformModuleDto
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Key         { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon        { get; set; } = string.Empty;
    public string Color       { get; set; } = string.Empty;
    public bool   IsActive    { get; set; }
    public int    SortOrder   { get; set; }
}

public class CreatePlatformModuleDto
{
    public string Name        { get; set; } = string.Empty;
    public string Key         { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon        { get; set; } = string.Empty;
    public string Color       { get; set; } = "#6c757d";
    public int    SortOrder   { get; set; }
}

public class UpdatePlatformModuleDto
{
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon        { get; set; } = string.Empty;
    public string Color       { get; set; } = string.Empty;
    public bool   IsActive    { get; set; }
    public int    SortOrder   { get; set; }
}

public class OrgModuleAssignmentDto
{
    public int        OrganizationId { get; set; }
    public List<int>  ModuleIds      { get; set; } = new();
}

public class OrgModuleStatusDto
{
    public int    OrganizationId   { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public List<OrgModuleItemDto> Modules { get; set; } = new();
}

public class OrgModuleItemDto
{
    public int    ModuleId   { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleKey  { get; set; } = string.Empty;
    public string Icon       { get; set; } = string.Empty;
    public string Color      { get; set; } = string.Empty;
    public bool   IsEnabled  { get; set; }
}
