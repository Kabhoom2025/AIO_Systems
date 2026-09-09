using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>A short-lived OTP code issued for one app+recipient pair (public Launch page OTP
/// Validation setting) - Recipient is an email address or phone number depending on Channel. Not
/// ITenantScoped - requested anonymously (guest/public forms have no current-user/org context),
/// so lookups key off AppId+Channel+Recipient+CodeHash directly instead of relying on the tenant
/// query filter. "Mobile & Email with different OTP Validations" issues two independent
/// challenges (one per channel), each verified separately.</summary>
public class AppOtpChallenge : BaseEntity
{
    public int AppId { get; set; }
    public AppOtpChannel Channel { get; set; } = AppOtpChannel.Email;
    public string Recipient { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Consumed { get; set; }
}
