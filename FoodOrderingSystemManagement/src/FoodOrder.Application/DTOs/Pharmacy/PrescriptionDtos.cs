namespace FoodOrder.Application.DTOs.Pharmacy;

public class PrescriptionDto
{
    public int      Id               { get; set; }
    public string   PatientName      { get; set; } = string.Empty;
    public string?  PatientPhone     { get; set; }
    public int?     PatientAge       { get; set; }
    public string   DoctorName       { get; set; } = string.Empty;
    public string?  DoctorRegNo      { get; set; }
    public string?  HospitalName     { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string?  ImageBase64      { get; set; }
    public string   Status           { get; set; } = string.Empty;
    public string?  Notes            { get; set; }
    public DateTime CreatedDate      { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new();
}

public class PrescriptionItemDto
{
    public int    Id           { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage       { get; set; } = string.Empty;
    public string Duration     { get; set; } = string.Empty;
    public int    Quantity     { get; set; }
    public bool   IsDispensed  { get; set; }
}

public class CreatePrescriptionDto
{
    public string   PatientName      { get; set; } = string.Empty;
    public string?  PatientPhone     { get; set; }
    public int?     PatientAge       { get; set; }
    public string   DoctorName       { get; set; } = string.Empty;
    public string?  DoctorRegNo      { get; set; }
    public string?  HospitalName     { get; set; }
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    public string?  ImageBase64      { get; set; }
    public string?  Notes            { get; set; }
    public List<CreatePrescriptionItemDto> Items { get; set; } = new();
}

public class CreatePrescriptionItemDto
{
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage       { get; set; } = string.Empty;
    public string Duration     { get; set; } = string.Empty;
    public int    Quantity     { get; set; }
}
