namespace NovaERP.Application.DTOs;

public class TrialBalanceLineDto
{
    public string  Code   { get; set; } = string.Empty;
    public string  Name   { get; set; } = string.Empty;
    public string  Type   { get; set; } = string.Empty;
    public decimal Debit  { get; set; }
    public decimal Credit { get; set; }
}

public class TrialBalanceDto
{
    public List<TrialBalanceLineDto> Lines { get; set; } = new();
    public decimal TotalDebit  { get; set; }
    public decimal TotalCredit { get; set; }
}

public class StatusSummaryRowDto
{
    public string  Status { get; set; } = string.Empty;
    public int     Count  { get; set; }
    public decimal Total  { get; set; }
}

public class StatusSummaryReportDto
{
    public List<StatusSummaryRowDto> Rows { get; set; } = new();
    public decimal GrandTotal { get; set; }
}
