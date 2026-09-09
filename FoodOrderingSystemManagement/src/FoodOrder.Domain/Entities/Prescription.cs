namespace FoodOrder.Domain.Entities;

public class Prescription : BaseEntity
{
    public int     OrganizationId  { get; set; }
    public string  PatientName     { get; set; } = string.Empty;
    public string? PatientPhone    { get; set; }
    public int?    PatientAge      { get; set; }
    public string  DoctorName      { get; set; } = string.Empty;
    public string? DoctorRegNo     { get; set; }
    public string? HospitalName    { get; set; }
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    public string?  ImageBase64    { get; set; }   // uploaded photo
    public string   Status         { get; set; } = "Pending";  // Pending | Dispensed | Partial
    public string?  Notes          { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
