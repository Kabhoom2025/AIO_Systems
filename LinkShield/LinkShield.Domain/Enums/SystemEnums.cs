namespace LinkShield.Domain.Enums;

public enum SystemRole
{
    SuperAdmin,
    Admin,
    SecurityAnalyst,
    User,
    ApiClient
}

public enum RiskLevel
{
    Low,
    Moderate,
    High,
    Critical
}

public enum ScanVerdict
{
    Unknown,
    Safe,
    LowRisk,
    Suspicious,
    Malicious
}

public enum ScanStatus
{
    Queued,
    UrlAnalysis,
    DomainAnalysis,
    DnsAnalysis,
    SslAnalysis,
    RedirectAnalysis,
    ThreatIntelligence,
    BrandDetection,
    MlAnalysis,
    RiskScoring,
    Completed,
    Failed
}

public enum ThreatIntelligenceStatus
{
    NoThreatDetected,
    Suspicious,
    ConfirmedMalicious,
    Error,
    Unavailable
}

public enum RiskFactorCategory
{
    UrlAnalysis,
    DomainIntelligence,
    DnsAnalysis,
    SslAnalysis,
    RedirectAnalysis,
    ThreatIntelligence,
    BrandImpersonation,
    MachineLearning
}

public enum NotificationType
{
    ScanCompleted,
    ThreatDetected,
    SystemAlert,
    AccountSecurity
}

public enum NotificationChannel
{
    InApp,
    Email,
    SignalR
}

public enum AuditAction
{
    Login,
    Logout,
    Register,
    PasswordReset,
    ScanCreated,
    ScanDeleted,
    RoleChanged,
    RiskRuleChanged,
    BrandProfileChanged,
    ApiKeyCreated,
    ApiKeyRevoked,
    SystemSettingChanged
}
