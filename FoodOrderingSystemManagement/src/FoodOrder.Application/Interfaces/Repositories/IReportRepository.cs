using FoodOrder.Application.DTOs.Report;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<SummaryStatsDto>             GetSummaryStatsAsync();
    Task<DailySalesDto>               GetDailySalesAsync(DateTime date);
    Task<IReadOnlyList<TopItemDto>>   GetTopItemsAsync(DateTime from, DateTime to, int limit = 10);
    Task<IReadOnlyList<PeakHourDto>>  GetPeakHoursAsync(DateTime date);
}
