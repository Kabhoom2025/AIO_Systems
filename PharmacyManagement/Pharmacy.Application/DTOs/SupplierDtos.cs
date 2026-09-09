namespace Pharmacy.Application.DTOs;

public class SupplierDto
{
    public int     Id            { get; set; }
    public string  Name          { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone         { get; set; }
    public string? Email         { get; set; }
    public string? Address       { get; set; }
    public string? GstNumber     { get; set; }
    public string? PaymentTerms  { get; set; }
    public decimal CreditLimit   { get; set; }
    public bool    IsActive      { get; set; }
}

public class CreateSupplierDto
{
    public string  Name          { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone         { get; set; }
    public string? Email         { get; set; }
    public string? Address       { get; set; }
    public string? GstNumber     { get; set; }
    public string? PaymentTerms  { get; set; }
    public decimal CreditLimit   { get; set; }
}

public class UpdateSupplierDto : CreateSupplierDto
{
    public bool IsActive { get; set; } = true;
}
