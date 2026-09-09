namespace FoodOrder.Application.DTOs.Report;

public class SummaryStatsDto
{
    public decimal TodayRevenue  { get; set; }
    public int     TodayOrders   { get; set; }
    public decimal WeekRevenue   { get; set; }
    public int     WeekOrders    { get; set; }
    public decimal MonthRevenue  { get; set; }
    public int     MonthOrders   { get; set; }
}

public class DailySalesDto
{
    public DateTime  Date             { get; set; }
    public decimal   TotalRevenue     { get; set; }
    public int       OrderCount       { get; set; }
    public decimal   AvgOrderValue    { get; set; }
    public int       CompletedOrders  { get; set; }
    public int       CancelledOrders  { get; set; }
    public List<HourlyBreakdownDto> HourlyBreakdown { get; set; } = new();
}

public class HourlyBreakdownDto
{
    public int     Hour       { get; set; }
    public string  Label      { get; set; } = string.Empty;
    public int     OrderCount { get; set; }
    public decimal Revenue    { get; set; }
}

public class TopItemDto
{
    public int     Rank         { get; set; }
    public string  ItemName     { get; set; } = string.Empty;
    public string  CategoryName { get; set; } = string.Empty;
    public int     QuantitySold { get; set; }
    public decimal Revenue      { get; set; }
}

public class PeakHourDto
{
    public int     Hour       { get; set; }
    public string  Label      { get; set; } = string.Empty;
    public int     OrderCount { get; set; }
    public decimal Revenue    { get; set; }
}
