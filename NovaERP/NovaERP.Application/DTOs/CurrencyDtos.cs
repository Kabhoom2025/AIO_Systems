namespace NovaERP.Application.DTOs;

public class CurrencyDto
{
    public int    Id            { get; set; }
    public string Code          { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Symbol        { get; set; } = string.Empty;
    public int    DecimalPlaces { get; set; } = 2;
    public bool   IsActive      { get; set; }
}

public class CreateCurrencyDto
{
    public string Code          { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Symbol        { get; set; } = string.Empty;
    public int    DecimalPlaces { get; set; } = 2;
}

public class UpdateCurrencyDto
{
    public string Name          { get; set; } = string.Empty;
    public string Symbol        { get; set; } = string.Empty;
    public int    DecimalPlaces { get; set; } = 2;
    public bool   IsActive      { get; set; }
}
