namespace Pharmacy.Domain.Entities;

public class Appointment : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      PatientId      { get; set; }
    public int      DoctorId       { get; set; }
    public int      BranchId       { get; set; }
    public DateTime PreferredDate  { get; set; }
    public string   TimeSlot       { get; set; } = string.Empty; // Morning | Afternoon | Evening
    public string   Status         { get; set; } = "Requested";  // Requested | Confirmed | Completed | Cancelled
    public string?  Notes          { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor  Doctor  { get; set; } = null!;
    public Branch  Branch  { get; set; } = null!;
}
