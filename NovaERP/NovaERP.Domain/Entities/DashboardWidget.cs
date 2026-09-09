namespace NovaERP.Domain.Entities;

/// <summary>One widget on a Dashboard. WidgetType drives which aggregate WidgetDataService
/// computes for it — no live data is stored here, only layout (SizeOption/DisplayOrder).</summary>
public class DashboardWidget : BaseEntity
{
    public int    DashboardId { get; set; }
    public string WidgetType  { get; set; } = string.Empty;
    public string Title       { get; set; } = string.Empty;
    public string SizeOption  { get; set; } = "Medium"; // Small | Medium | Large
    public int    DisplayOrder { get; set; }

    public Dashboard Dashboard { get; set; } = null!;
}
