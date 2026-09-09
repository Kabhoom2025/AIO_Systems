namespace Pharmacy.Application.DTOs;

public class PatientDto
{
    public int       Id                    { get; set; }
    public string    Name                  { get; set; } = string.Empty;
    public string?   Phone                 { get; set; }
    public string?   Email                 { get; set; }
    public DateTime? DateOfBirth           { get; set; }
    public string?   Gender                { get; set; }
    public string?   Address               { get; set; }
    public string?   BloodGroup            { get; set; }
    public string?   Allergies             { get; set; }
    public string?   ChronicDiseases       { get; set; }
    public string?   EmergencyContactName  { get; set; }
    public string?   EmergencyContactPhone { get; set; }
    public string?   InsuranceProvider     { get; set; }
    public string?   InsurancePolicyNumber { get; set; }
    public bool      IsActive              { get; set; }
    public bool      IsVerified            { get; set; }
}

public class CreatePatientDto
{
    public string    Name                  { get; set; } = string.Empty;
    public string?   Phone                 { get; set; }
    public string?   Email                 { get; set; }
    public DateTime? DateOfBirth           { get; set; }
    public string?   Gender                { get; set; }
    public string?   Address               { get; set; }
    public string?   BloodGroup            { get; set; }
    public string?   Allergies             { get; set; }
    public string?   ChronicDiseases       { get; set; }
    public string?   EmergencyContactName  { get; set; }
    public string?   EmergencyContactPhone { get; set; }
    public string?   InsuranceProvider     { get; set; }
    public string?   InsurancePolicyNumber { get; set; }
}

public class UpdatePatientDto : CreatePatientDto
{
    public bool IsActive { get; set; } = true;
}

public class UpdatePatientProfileDto
{
    public string    Name                  { get; set; } = string.Empty;
    public string?   Email                 { get; set; }
    public DateTime? DateOfBirth           { get; set; }
    public string?   Gender                { get; set; }
    public string?   Address               { get; set; }
    public string?   BloodGroup            { get; set; }
    public string?   Allergies             { get; set; }
    public string?   ChronicDiseases       { get; set; }
    public string?   EmergencyContactName  { get; set; }
    public string?   EmergencyContactPhone { get; set; }
    public string?   InsuranceProvider     { get; set; }
    public string?   InsurancePolicyNumber { get; set; }
}
