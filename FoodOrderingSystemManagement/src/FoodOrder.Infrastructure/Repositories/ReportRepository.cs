using FoodOrder.Application.DTOs.Report;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Enums;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;
    public ReportRepository(AppDbContext db) => _db = db;

    public async Task<SummaryStatsDto> GetSummaryStatsAsync()
    {
        var now        = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart  = todayStart.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var orders = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= monthStart)
            .ToListAsync();

        var today = orders.Where(o => o.OrderDate >= todayStart).ToList();
        var week  = orders.Where(o => o.OrderDate >= weekStart).ToList();

        return new SummaryStatsDto
        {
            TodayRevenue = today.Sum(o => o.GrandTotal),
            TodayOrders  = today.Count,
            WeekRevenue  = week.Sum(o => o.GrandTotal),
            WeekOrders   = week.Count,
            MonthRevenue = orders.Sum(o => o.GrandTotal),
            MonthOrders  = orders.Count,
        };
    }

    public async Task<DailySalesDto> GetDailySalesAsync(DateTime date)
    {
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd   = dayStart.AddDays(1);

        var orders = await _db.Orders
            .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd)
            .ToListAsync();

        var completed = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var cancelled = orders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

        var hourly = completed
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new HourlyBreakdownDto
            {
                Hour       = g.Key,
                Label      = FormatHour(g.Key),
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.GrandTotal),
            })
            .OrderBy(h => h.Hour)
            .ToList();

        return new DailySalesDto
        {
            Date            = date.Date,
            TotalRevenue    = completed.Sum(o => o.GrandTotal),
            OrderCount      = completed.Count,
            AvgOrderValue   = completed.Count > 0 ? completed.Average(o => o.GrandTotal) : 0,
            CompletedOrders = completed.Count,
            CancelledOrders = cancelled.Count,
            HourlyBreakdown = hourly,
        };
    }

    public async Task<IReadOnlyList<TopItemDto>> GetTopItemsAsync(DateTime from, DateTime to, int limit = 10)
    {
        var fromUtc = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Utc);

        var items = await _db.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.FoodItem).ThenInclude(fi => fi.Category)
            .Where(oi => oi.Order.Status == OrderStatus.Completed
                      && oi.Order.OrderDate >= fromUtc
                      && oi.Order.OrderDate < toUtc)
            .ToListAsync();

        var grouped = items
            .GroupBy(oi => new
            {
                oi.FoodItemId,
                oi.FoodItem.ItemName,
                CategoryName = oi.FoodItem.Category?.CategoryName ?? "Unknown",
            })
            .Select(g => new
            {
                g.Key.ItemName,
                g.Key.CategoryName,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue      = g.Sum(x => x.LineTotal),
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(limit)
            .ToList();

        return grouped.Select((x, i) => new TopItemDto
        {
            Rank         = i + 1,
            ItemName     = x.ItemName,
            CategoryName = x.CategoryName,
            QuantitySold = x.QuantitySold,
            Revenue      = x.Revenue,
        }).ToList();
    }

    public async Task<IReadOnlyList<PeakHourDto>> GetPeakHoursAsync(DateTime date)
    {
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd   = dayStart.AddDays(1);

        var orders = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed
                     && o.OrderDate >= dayStart
                     && o.OrderDate < dayEnd)
            .ToListAsync();

        return orders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new PeakHourDto
            {
                Hour       = g.Key,
                Label      = FormatHour(g.Key),
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.GrandTotal),
            })
            .OrderBy(h => h.Hour)
            .ToList();
    }

    private static string FormatHour(int h)
    {
        if (h == 0)  return "12 AM";
        if (h < 12)  return $"{h} AM";
        if (h == 12) return "12 PM";
        return $"{h - 12} PM";
    }
}
