namespace Pharmacy.Domain.Entities;

public class Doctor : BaseEntity
{
    public int     OrganizationId     { get; set; }
    public string  Name               { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Specialty          { get; set; }
    public string? Phone              { get; set; }
    public string? Email              { get; set; }
    public string? HospitalName       { get; set; }
    public bool    IsActive           { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}
