namespace FoodOrder.Application.DTOs.Settings;

public class UpdateSettingsDto
{
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
    public decimal PointsPerRupee   { get; set; }
    public decimal PointsRedeemRate { get; set; }
    public string? UpiId { get; set; }
    public bool PromoAutoScroll { get; set; }

    // ── POS Settings ──────────────────────────────────────────────────────────
    public string PosDefaultOrderType     { get; set; } = "DineIn";
    public bool   PosAutoPrintBill        { get; set; }
    public bool   PosEnableTips           { get; set; }
    public bool   PosRequireCustomerPhone { get; set; }

    // ── Menu Settings ─────────────────────────────────────────────────────────
    public bool MenuShowImages       { get; set; }
    public bool MenuShowDescriptions { get; set; }
    public bool MenuShowOutOfStock   { get; set; }

    // ── Tax Configuration (extended) ─────────────────────────────────────────
    public string TaxName      { get; set; } = "GST";
    public bool   TaxInclusive { get; set; }

    // ── Printer Settings ─────────────────────────────────────────────────────
    public bool    PrinterEnabled   { get; set; }
    public string  PrinterType      { get; set; } = "Thermal";
    public string? PrinterIp        { get; set; }
    public int     PrinterPort      { get; set; }
    public bool    PrinterAutoPrint { get; set; }

    // ── Payment Gateway (extended) ────────────────────────────────────────────
    public string? RazorpayKeyId        { get; set; }
    public string? RazorpayKeySecret    { get; set; }
    public bool    PaymentCashEnabled   { get; set; }
    public bool    PaymentCardEnabled   { get; set; }
    public bool    PaymentOnlineEnabled { get; set; }

    // ── Email Configuration ───────────────────────────────────────────────────
    public string? SmtpHost      { get; set; }
    public int     SmtpPort      { get; set; }
    public string? SmtpUsername  { get; set; }
    public string? SmtpPassword  { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName  { get; set; }
    public bool    SmtpSsl       { get; set; }

    // ── Backup Settings ───────────────────────────────────────────────────────
    public bool    AutoBackupEnabled    { get; set; }
    public string? BackupEmail          { get; set; }
    public int     BackupFrequencyHours { get; set; }

    // ── Theme Settings ────────────────────────────────────────────────────────
    public string ThemePrimaryColor { get; set; } = "#bf360c";
    public string ThemeMode         { get; set; } = "light";

    // ── Feature Toggles ───────────────────────────────────────────────────────
    public bool FeatOnlineOrdering    { get; set; }
    public bool FeatTableReservations { get; set; }
    public bool FeatLoyaltyProgram    { get; set; }
    public bool FeatSmsNotifications  { get; set; }
    public bool FeatWhatsappAlerts    { get; set; }
    public bool FeatDeliveryModule    { get; set; }
    public bool FeatKitchenDisplay    { get; set; }
    public bool FeatPromotionsCoupons { get; set; }

    // ── Third-party Integrations ──────────────────────────────────────────────
    public string? DunzoApiKey       { get; set; }
    public string? SwiggyApiKey      { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? MailchimpApiKey   { get; set; }
    public string? SlackWebhookUrl   { get; set; }

    // ── Google Maps ───────────────────────────────────────────────────────────
    public string?  GoogleMapsApiKey    { get; set; }
    public decimal? RestaurantLatitude  { get; set; }
    public decimal? RestaurantLongitude { get; set; }

    // ── SSO Provider Credentials ──────────────────────────────────────────────
    public string? SsoGoogleClientId        { get; set; }
    public string? SsoGoogleClientSecret    { get; set; }
    public string? SsoMicrosoftClientId     { get; set; }
    public string? SsoMicrosoftClientSecret { get; set; }
    public string? SsoMicrosoftTenantId     { get; set; }
    public string? SsoFacebookAppId         { get; set; }
    public string? SsoFacebookAppSecret     { get; set; }
    public string? SsoGitHubClientId        { get; set; }
    public string? SsoGitHubClientSecret    { get; set; }
}
