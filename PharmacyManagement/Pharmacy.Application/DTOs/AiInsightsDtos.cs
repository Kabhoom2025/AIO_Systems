namespace Pharmacy.Application.DTOs;

public class ReorderSuggestionDto
{
    public int     MedicineId          { get; set; }
    public string  MedicineName        { get; set; } = string.Empty;
    public int     CurrentStock        { get; set; }
    public decimal AvgDailySales       { get; set; }
    public decimal? DaysOfStockLeft    { get; set; }
    public int     SuggestedReorderQty { get; set; }
}

public class ExpiryRiskDto
{
    public int      MedicineId             { get; set; }
    public string   MedicineName           { get; set; } = string.Empty;
    public int      BatchId                { get; set; }
    public string   BatchNumber            { get; set; } = string.Empty;
    public int      Quantity               { get; set; }
    public int      DaysToExpiry           { get; set; }
    public decimal  AvgDailySales          { get; set; }
    public decimal? ProjectedDaysToSellOut { get; set; }
    public string   RiskLevel              { get; set; } = string.Empty;
}
