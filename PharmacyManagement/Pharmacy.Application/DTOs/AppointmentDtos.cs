namespace Pharmacy.Application.DTOs;

public class CreateAppointmentDto
{
    public int      DoctorId      { get; set; }
    public int      BranchId      { get; set; }
    public DateTime PreferredDate { get; set; }
    public string   TimeSlot      { get; set; } = string.Empty;
    public string?  Notes         { get; set; }
}

public class UpdateAppointmentStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class AppointmentDto
{
    public int      Id              { get; set; }
    public int      PatientId       { get; set; }
    public string   PatientName     { get; set; } = string.Empty;
    public string?  PatientPhone    { get; set; }
    public int      DoctorId        { get; set; }
    public string   DoctorName      { get; set; } = string.Empty;
    public string?  DoctorSpecialty { get; set; }
    public int      BranchId        { get; set; }
    public string   BranchName      { get; set; } = string.Empty;
    public DateTime PreferredDate   { get; set; }
    public string   TimeSlot        { get; set; } = string.Empty;
    public string   Status          { get; set; } = string.Empty;
    public string?  Notes           { get; set; }
    public DateTime CreatedDate     { get; set; }
}
