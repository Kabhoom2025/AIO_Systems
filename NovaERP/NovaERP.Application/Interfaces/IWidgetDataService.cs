using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IWidgetDataService
{
    Task<WidgetDataDto> GetDataAsync(int orgId, string widgetType);
}
