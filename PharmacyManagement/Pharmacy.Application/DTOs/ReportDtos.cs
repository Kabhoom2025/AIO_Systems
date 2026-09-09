namespace Pharmacy.Application.DTOs;

public class ProfitLossDto
{
    public DateTime From             { get; set; }
    public DateTime To               { get; set; }
    public decimal  Revenue          { get; set; }
    public decimal  CostOfGoodsSold  { get; set; }
    public decimal  GrossProfit      { get; set; }
    public decimal  Expenses         { get; set; }
    public decimal  NetProfit        { get; set; }
}

public class GstReportDto
{
    public DateTime From          { get; set; }
    public DateTime To            { get; set; }
    public decimal  OutputTax     { get; set; }
    public decimal  InputTax      { get; set; }
    public decimal  NetGstPayable { get; set; }
}

public class CashBookEntryDto
{
    public DateTime Date            { get; set; }
    public string   Description     { get; set; } = string.Empty;
    public string   Type            { get; set; } = string.Empty; // In | Out
    public decimal  Amount          { get; set; }
    public decimal  RunningBalance  { get; set; }
}

public class SalesTrendPointDto
{
    public DateTime Date   { get; set; }
    public decimal  Amount { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string  Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SalesSummaryDto
{
    public DateTime From             { get; set; }
    public DateTime To               { get; set; }
    public decimal  TotalSales       { get; set; }
    public int      TransactionCount { get; set; }
    public decimal  AverageTicket    { get; set; }
    public List<SalesTrendPointDto>          DailyTrend    { get; set; } = new();
    public List<PaymentMethodBreakdownDto>   ByPaymentMethod { get; set; } = new();
}

public class TopMedicineDto
{
    public int     MedicineId   { get; set; }
    public string  MedicineName { get; set; } = string.Empty;
    public int     QuantitySold { get; set; }
    public decimal Revenue      { get; set; }
}

public class InventoryValuationDto
{
    public int     MedicineId    { get; set; }
    public string  MedicineName  { get; set; } = string.Empty;
    public int     TotalStock    { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal StockValue    { get; set; }
}

public class InventoryValuationReportDto
{
    public List<InventoryValuationDto> Items      { get; set; } = new();
    public decimal                     GrandTotal { get; set; }
}
