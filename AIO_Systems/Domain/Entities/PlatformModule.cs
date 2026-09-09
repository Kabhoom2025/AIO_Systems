namespace AIO_Systems.Domain.Entities;

public class PlatformModule : BaseEntity
{
    public string Name        { get; set; } = string.Empty;
    public string Key         { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon        { get; set; } = string.Empty;
    public string Color       { get; set; } = "#6c757d";
    public bool   IsActive    { get; set; } = true;
    public int    SortOrder   { get; set; }

    public ICollection<OrganizationModule> OrganizationModules { get; set; } = new List<OrganizationModule>();
}
