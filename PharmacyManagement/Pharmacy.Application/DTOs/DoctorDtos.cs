namespace Pharmacy.Application.DTOs;

public class DoctorDto
{
    public int     Id                 { get; set; }
    public string  Name               { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Specialty          { get; set; }
    public string? Phone              { get; set; }
    public string? Email              { get; set; }
    public string? HospitalName       { get; set; }
    public bool    IsActive           { get; set; }
}

public class CreateDoctorDto
{
    public string  Name               { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Specialty          { get; set; }
    public string? Phone              { get; set; }
    public string? Email              { get; set; }
    public string? HospitalName       { get; set; }
}

public class UpdateDoctorDto : CreateDoctorDto
{
    public bool IsActive { get; set; } = true;
}

public class PublicDoctorDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Specialty    { get; set; }
    public string? HospitalName { get; set; }
}
