export interface Settings {
  id: number;
  taxPercentage: number;
  restaurantName: string;
  address: string;
  phone: string;
  gstNumber: string;
  msg91ApiKey: string | null;
  msg91SenderId: string | null;
  ultraMsgInstanceId: string | null;
  ultraMsgToken: string | null;
  logoUrl: string | null;
  pointsPerRupee: number;
  pointsRedeemRate: number;
  upiId?: string | null;
  promoAutoScroll: boolean;

  // POS Settings
  posDefaultOrderType: string;
  posAutoPrintBill: boolean;
  posEnableTips: boolean;
  posRequireCustomerPhone: boolean;

  // Menu Settings
  menuShowImages: boolean;
  menuShowDescriptions: boolean;
  menuShowOutOfStock: boolean;

  // Tax (extended)
  taxName: string;
  taxInclusive: boolean;

  // Printer Settings
  printerEnabled: boolean;
  printerType: string;
  printerIp?: string | null;
  printerPort: number;
  printerAutoPrint: boolean;

  // Payment Gateway (extended)
  razorpayKeyId?: string | null;
  razorpayKeySecret?: string | null;
  paymentCashEnabled: boolean;
  paymentCardEnabled: boolean;
  paymentOnlineEnabled: boolean;

  // Email Configuration
  smtpHost?: string | null;
  smtpPort: number;
  smtpUsername?: string | null;
  smtpPassword?: string | null;
  smtpFromEmail?: string | null;
  smtpFromName?: string | null;
  smtpSsl: boolean;

  // Backup Settings
  autoBackupEnabled: boolean;
  backupEmail?: string | null;
  backupFrequencyHours: number;

  // Theme Settings
  themePrimaryColor: string;
  themeMode: string;

  // Feature Toggles
  featOnlineOrdering: boolean;
  featTableReservations: boolean;
  featLoyaltyProgram: boolean;
  featSmsNotifications: boolean;
  featWhatsappAlerts: boolean;
  featDeliveryModule: boolean;
  featKitchenDisplay: boolean;
  featPromotionsCoupons: boolean;

  // Third-party Integrations
  dunzoApiKey?: string | null;
  swiggyApiKey?: string | null;
  googleAnalyticsId?: string | null;
  mailchimpApiKey?: string | null;
  slackWebhookUrl?: string | null;

  // Google Maps
  googleMapsApiKey?: string | null;
  restaurantLatitude?: number | null;
  restaurantLongitude?: number | null;

  // SSO Provider Credentials
  ssoGoogleClientId?: string | null;
  ssoGoogleClientSecret?: string | null;
  ssoMicrosoftClientId?: string | null;
  ssoMicrosoftClientSecret?: string | null;
  ssoMicrosoftTenantId?: string | null;
  ssoFacebookAppId?: string | null;
  ssoFacebookAppSecret?: string | null;
  ssoGitHubClientId?: string | null;
  ssoGitHubClientSecret?: string | null;

  // CRM Integration
  crmProvider?: string | null;
  crmApiKey?: string | null;
  crmWebhookUrl?: string | null;

  // Accounting Integration
  accountingProvider?: string | null;
  accountingApiKey?: string | null;
  accountingWebhookUrl?: string | null;

  // ERP Integration
  erpProvider?: string | null;
  erpApiKey?: string | null;
  erpWebhookUrl?: string | null;
}

export interface UpdateSettingsRequest {
  taxPercentage: number;
  restaurantName: string;
  address?: string;
  phone?: string;
  gstNumber?: string;
  msg91ApiKey?: string | null;
  msg91SenderId?: string | null;
  ultraMsgInstanceId?: string | null;
  ultraMsgToken?: string | null;
  logoUrl?: string | null;
  pointsPerRupee?: number;
  pointsRedeemRate?: number;
  upiId?: string | null;
  promoAutoScroll?: boolean;

  // POS Settings
  posDefaultOrderType?: string;
  posAutoPrintBill?: boolean;
  posEnableTips?: boolean;
  posRequireCustomerPhone?: boolean;

  // Menu Settings
  menuShowImages?: boolean;
  menuShowDescriptions?: boolean;
  menuShowOutOfStock?: boolean;

  // Tax (extended)
  taxName?: string;
  taxInclusive?: boolean;

  // Printer Settings
  printerEnabled?: boolean;
  printerType?: string;
  printerIp?: string | null;
  printerPort?: number;
  printerAutoPrint?: boolean;

  // Payment Gateway (extended)
  razorpayKeyId?: string | null;
  razorpayKeySecret?: string | null;
  paymentCashEnabled?: boolean;
  paymentCardEnabled?: boolean;
  paymentOnlineEnabled?: boolean;

  // Email Configuration
  smtpHost?: string | null;
  smtpPort?: number;
  smtpUsername?: string | null;
  smtpPassword?: string | null;
  smtpFromEmail?: string | null;
  smtpFromName?: string | null;
  smtpSsl?: boolean;

  // Backup Settings
  autoBackupEnabled?: boolean;
  backupEmail?: string | null;
  backupFrequencyHours?: number;

  // Theme Settings
  themePrimaryColor?: string;
  themeMode?: string;

  // Feature Toggles
  featOnlineOrdering?: boolean;
  featTableReservations?: boolean;
  featLoyaltyProgram?: boolean;
  featSmsNotifications?: boolean;
  featWhatsappAlerts?: boolean;
  featDeliveryModule?: boolean;
  featKitchenDisplay?: boolean;
  featPromotionsCoupons?: boolean;

  // Third-party Integrations
  dunzoApiKey?: string | null;
  swiggyApiKey?: string | null;
  googleAnalyticsId?: string | null;
  mailchimpApiKey?: string | null;
  slackWebhookUrl?: string | null;

  // Google Maps
  googleMapsApiKey?: string | null;
  restaurantLatitude?: number | null;
  restaurantLongitude?: number | null;

  // SSO Provider Credentials
  ssoGoogleClientId?: string | null;
  ssoGoogleClientSecret?: string | null;
  ssoMicrosoftClientId?: string | null;
  ssoMicrosoftClientSecret?: string | null;
  ssoMicrosoftTenantId?: string | null;
  ssoFacebookAppId?: string | null;
  ssoFacebookAppSecret?: string | null;
  ssoGitHubClientId?: string | null;
  ssoGitHubClientSecret?: string | null;

  // CRM Integration
  crmProvider?: string | null;
  crmApiKey?: string | null;
  crmWebhookUrl?: string | null;

  // Accounting Integration
  accountingProvider?: string | null;
  accountingApiKey?: string | null;
  accountingWebhookUrl?: string | null;

  // ERP Integration
  erpProvider?: string | null;
  erpApiKey?: string | null;
  erpWebhookUrl?: string | null;
}
