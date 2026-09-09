namespace Pharmacy.Application.DTOs;

public class OrganizationDto
{
    public int     Id                  { get; set; }
    public string  Name                { get; set; } = string.Empty;
    public string? Address             { get; set; }
    public string? Phone               { get; set; }
    public string? Email               { get; set; }
    public string? LicenseNo           { get; set; }
    public string  Currency            { get; set; } = "INR";
    public string? GstNumber           { get; set; }
    public string  InvoiceNumberPrefix { get; set; } = "INV";
}

public class PublicOrganizationDto
{
    public int     Id    { get; set; }
    public string  Name  { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class UpdateOrganizationDto
{
    public string  Name                { get; set; } = string.Empty;
    public string? Address             { get; set; }
    public string? Phone               { get; set; }
    public string? Email               { get; set; }
    public string? LicenseNo           { get; set; }
    public string  Currency            { get; set; } = "INR";
    public string? GstNumber           { get; set; }
    public string  InvoiceNumberPrefix { get; set; } = "INV";
}
