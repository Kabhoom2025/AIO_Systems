namespace AIO_Systems.Domain.Entities;

public class OrganizationModule
{
    public int  OrganizationId    { get; set; }
    public int  PlatformModuleId  { get; set; }
    public bool IsEnabled         { get; set; } = true;

    public Organization    Organization   { get; set; } = null!;
    public PlatformModule  PlatformModule { get; set; } = null!;
}
