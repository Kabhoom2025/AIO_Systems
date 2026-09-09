namespace NovaERP.Application.DTOs;

public class DashboardWidgetDto
{
    public int    Id           { get; set; }
    public string WidgetType   { get; set; } = string.Empty;
    public string Title        { get; set; } = string.Empty;
    public string SizeOption   { get; set; } = string.Empty;
    public int    DisplayOrder { get; set; }
}

public class DashboardDto
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public bool   IsDefault { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
}

public class CreateDashboardDto
{
    public string Name      { get; set; } = string.Empty;
    public bool   IsDefault { get; set; }
}

public class UpdateDashboardDto
{
    public string Name      { get; set; } = string.Empty;
    public bool   IsDefault { get; set; }
}

public class AddWidgetDto
{
    public string WidgetType { get; set; } = string.Empty;
    public string Title      { get; set; } = string.Empty;
    public string SizeOption { get; set; } = "Medium";
}

public class WidgetLayoutEntryDto
{
    public int    WidgetId     { get; set; }
    public int    DisplayOrder { get; set; }
    public string SizeOption   { get; set; } = "Medium";
}

public class ReorderWidgetsDto
{
    public List<WidgetLayoutEntryDto> Widgets { get; set; } = new();
}

/// <summary>A generic label/value bucket shape reused across all widget types — Total is only
/// populated for sum-style widgets (e.g. PosSalesTotal), null for count-style widgets.</summary>
public class WidgetDataDto
{
    public List<string>  Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
    public decimal?       Total  { get; set; }
}
