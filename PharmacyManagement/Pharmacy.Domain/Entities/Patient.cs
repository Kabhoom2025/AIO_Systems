namespace Pharmacy.Domain.Entities;

public class Patient : BaseEntity
{
    public int       OrganizationId          { get; set; }
    public string    Name                    { get; set; } = string.Empty;
    public string?   Phone                   { get; set; }
    public string?   Email                   { get; set; }
    public DateTime? DateOfBirth             { get; set; }
    public string?   Gender                  { get; set; }
    public string?   Address                 { get; set; }
    public string?   BloodGroup              { get; set; }
    public string?   Allergies               { get; set; }
    public string?   ChronicDiseases         { get; set; }
    public string?   EmergencyContactName    { get; set; }
    public string?   EmergencyContactPhone   { get; set; }
    public string?   InsuranceProvider       { get; set; }
    public string?   InsurancePolicyNumber   { get; set; }
    public bool      IsActive                { get; set; } = true;
    public bool      IsVerified              { get; set; } = false;

    public Organization Organization { get; set; } = null!;
}
