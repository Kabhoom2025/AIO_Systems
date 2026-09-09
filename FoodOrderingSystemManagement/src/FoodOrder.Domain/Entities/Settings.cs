namespace FoodOrder.Domain.Entities;

public class Settings
{
    public int Id { get; set; }
    public decimal TaxPercentage { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? GSTNumber { get; set; }
    public string? Msg91ApiKey { get; set; }
    public string? Msg91SenderId { get; set; }
    public string? UltraMsgInstanceId { get; set; }
    public string? UltraMsgToken { get; set; }
    public string? LogoUrl { get; set; }

    public decimal PointsPerRupee   { get; set; } = 1.0m;
    public decimal PointsRedeemRate { get; set; } = 0.25m;
    public string? UpiId { get; set; }
    public bool PromoAutoScroll { get; set; } = true;

    // ── POS Settings ──────────────────────────────────────────────────────────
    public string PosDefaultOrderType     { get; set; } = "DineIn";
    public bool   PosAutoPrintBill        { get; set; } = false;
    public bool   PosEnableTips           { get; set; } = false;
    public bool   PosRequireCustomerPhone { get; set; } = false;

    // ── Menu Settings ─────────────────────────────────────────────────────────
    public bool MenuShowImages       { get; set; } = true;
    public bool MenuShowDescriptions { get; set; } = true;
    public bool MenuShowOutOfStock   { get; set; } = true;

    // ── Tax Configuration (extended) ─────────────────────────────────────────
    public string TaxName      { get; set; } = "GST";
    public bool   TaxInclusive { get; set; } = false;

    // ── Printer Settings ─────────────────────────────────────────────────────
    public bool    PrinterEnabled    { get; set; } = false;
    public string  PrinterType       { get; set; } = "Thermal";
    public string? PrinterIp         { get; set; }
    public int     PrinterPort       { get; set; } = 9100;
    public bool    PrinterAutoPrint  { get; set; } = false;

    // ── Payment Gateway (extended) ────────────────────────────────────────────
    public string? RazorpayKeyId       { get; set; }
    public string? RazorpayKeySecret   { get; set; }
    public bool    PaymentCashEnabled  { get; set; } = true;
    public bool    PaymentCardEnabled  { get; set; } = true;
    public bool    PaymentOnlineEnabled{ get; set; } = false;

    // ── Email Configuration ───────────────────────────────────────────────────
    public string? SmtpHost      { get; set; }
    public int     SmtpPort      { get; set; } = 587;
    public string? SmtpUsername  { get; set; }
    public string? SmtpPassword  { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName  { get; set; }
    public bool    SmtpSsl       { get; set; } = true;

    // ── Backup Settings ───────────────────────────────────────────────────────
    public bool    AutoBackupEnabled      { get; set; } = false;
    public string? BackupEmail            { get; set; }
    public int     BackupFrequencyHours   { get; set; } = 24;

    // ── Theme Settings ────────────────────────────────────────────────────────
    public string ThemePrimaryColor { get; set; } = "#bf360c";
    public string ThemeMode         { get; set; } = "light";

    // ── Feature Toggles ───────────────────────────────────────────────────────
    public bool FeatOnlineOrdering    { get; set; } = true;
    public bool FeatTableReservations { get; set; } = false;
    public bool FeatLoyaltyProgram    { get; set; } = true;
    public bool FeatSmsNotifications  { get; set; } = false;
    public bool FeatWhatsappAlerts    { get; set; } = false;
    public bool FeatDeliveryModule    { get; set; } = true;
    public bool FeatKitchenDisplay    { get; set; } = true;
    public bool FeatPromotionsCoupons { get; set; } = false;

    // ── Third-party Integrations ──────────────────────────────────────────────
    public string? DunzoApiKey       { get; set; }
    public string? SwiggyApiKey      { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? MailchimpApiKey   { get; set; }
    public string? SlackWebhookUrl   { get; set; }

    // ── Google Maps ───────────────────────────────────────────────────────────
    public string?  GoogleMapsApiKey      { get; set; }
    public decimal? RestaurantLatitude    { get; set; }
    public decimal? RestaurantLongitude   { get; set; }

    // ── SSO Provider Credentials ──────────────────────────────────────────────
    public string? SsoGoogleClientId       { get; set; }
    public string? SsoGoogleClientSecret   { get; set; }
    public string? SsoMicrosoftClientId    { get; set; }
    public string? SsoMicrosoftClientSecret{ get; set; }
    public string? SsoMicrosoftTenantId    { get; set; }
    public string? SsoFacebookAppId        { get; set; }
    public string? SsoFacebookAppSecret    { get; set; }
    public string? SsoGitHubClientId       { get; set; }
    public string? SsoGitHubClientSecret   { get; set; }

    // ── CRM Integration ──────────────────────────────────────────────────────
    public string? CrmProvider    { get; set; }   // hubspot | zoho | custom
    public string? CrmApiKey      { get; set; }
    public string? CrmWebhookUrl  { get; set; }

    // ── Accounting Integration ────────────────────────────────────────────
    public string? AccountingProvider    { get; set; }   // quickbooks | xero | tally
    public string? AccountingApiKey      { get; set; }
    public string? AccountingWebhookUrl  { get; set; }

    // ── ERP Integration ───────────────────────────────────────────────────
    public string? ErpProvider    { get; set; }   // sap | oracle | netsuite | custom
    public string? ErpApiKey      { get; set; }
    public string? ErpWebhookUrl  { get; set; }

    // ── Branch (Organization) scoping ─────────────────────────────────────────
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
