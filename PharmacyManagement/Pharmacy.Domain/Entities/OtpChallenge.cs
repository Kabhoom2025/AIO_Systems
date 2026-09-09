namespace Pharmacy.Domain.Entities;

public class OtpChallenge : BaseEntity
{
    public string   Phone     { get; set; } = string.Empty;
    public string   Code      { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool     Consumed  { get; set; } = false;
    public int      Attempts  { get; set; } = 0;
}
