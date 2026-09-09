namespace NovaERP.Application.DTOs;

public class OrganizationSettingsDto
{
    public int    Id                   { get; set; }
    public string DefaultLanguageCode  { get; set; } = "en";
    public string DefaultCurrencyCode  { get; set; } = "INR";
    public string DefaultTimezone      { get; set; } = "Asia/Kolkata";
    public string DateFormat           { get; set; } = "dd/MM/yyyy";
    public string TimeFormat           { get; set; } = "HH:mm";
    public int    FiscalYearStartMonth { get; set; } = 4;
    public string InvoiceNumberPrefix  { get; set; } = "INV";
    public bool   TaxInclusivePricing  { get; set; }
}

public class UpdateOrganizationSettingsDto
{
    public string DefaultLanguageCode  { get; set; } = "en";
    public string DefaultCurrencyCode  { get; set; } = "INR";
    public string DefaultTimezone      { get; set; } = "Asia/Kolkata";
    public string DateFormat           { get; set; } = "dd/MM/yyyy";
    public string TimeFormat           { get; set; } = "HH:mm";
    public int    FiscalYearStartMonth { get; set; } = 4;
    public string InvoiceNumberPrefix  { get; set; } = "INV";
    public bool   TaxInclusivePricing  { get; set; }
}
