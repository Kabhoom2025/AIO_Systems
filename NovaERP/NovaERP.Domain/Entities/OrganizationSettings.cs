namespace NovaERP.Domain.Entities;

/// <summary>One row per Organization holding platform-level defaults (locale, currency,
/// fiscal calendar, invoicing). Module-specific settings (SMTP, payment gateway, POS, ...)
/// belong to those modules once built — this stays lean and foundation-only.</summary>
public class OrganizationSettings : BaseEntity
{
    public int    OrganizationId        { get; set; }
    public string DefaultLanguageCode   { get; set; } = "en";
    public string DefaultCurrencyCode   { get; set; } = "INR";
    public string DefaultTimezone       { get; set; } = "Asia/Kolkata";
    public string DateFormat            { get; set; } = "dd/MM/yyyy";
    public string TimeFormat            { get; set; } = "HH:mm";
    public int    FiscalYearStartMonth  { get; set; } = 4;
    public string InvoiceNumberPrefix   { get; set; } = "INV";
    public bool   TaxInclusivePricing   { get; set; }

    public Organization Organization { get; set; } = null!;
}
