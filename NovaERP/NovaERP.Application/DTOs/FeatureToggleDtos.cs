namespace NovaERP.Application.DTOs;

public class FeatureToggleDto
{
    public int    Id        { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public bool   IsEnabled { get; set; }
}

public class UpdateFeatureToggleDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public bool   IsEnabled { get; set; }
}
