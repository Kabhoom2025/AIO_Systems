namespace NovaERP.Application.DTOs;

public class ExchangeRateDto
{
    public int      Id               { get; set; }
    public string   FromCurrencyCode { get; set; } = string.Empty;
    public string   ToCurrencyCode   { get; set; } = string.Empty;
    public decimal  Rate             { get; set; }
    public DateTime EffectiveDate    { get; set; }
}

public class CreateExchangeRateDto
{
    public string   FromCurrencyCode { get; set; } = string.Empty;
    public string   ToCurrencyCode   { get; set; } = string.Empty;
    public decimal  Rate             { get; set; }
    public DateTime EffectiveDate    { get; set; }
}

public class UpdateExchangeRateDto
{
    public decimal  Rate          { get; set; }
    public DateTime EffectiveDate { get; set; }
}
