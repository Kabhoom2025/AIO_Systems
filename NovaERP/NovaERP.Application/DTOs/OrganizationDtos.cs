namespace NovaERP.Application.DTOs;

public class OrganizationDto
{
    public int     Id        { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string  Code      { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Address   { get; set; }
    public string? Phone     { get; set; }
    public string? Email     { get; set; }
    public string? Website   { get; set; }
    public string? TaxNumber { get; set; }
    public string  Timezone  { get; set; } = "Asia/Kolkata";
    public string  Currency  { get; set; } = "INR";
    public string? LogoUrl   { get; set; }
    public bool    IsActive  { get; set; }
}

public class UpdateOrganizationDto
{
    public string  Name      { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Address   { get; set; }
    public string? Phone     { get; set; }
    public string? Email     { get; set; }
    public string? Website   { get; set; }
    public string? TaxNumber { get; set; }
    public string  Timezone  { get; set; } = "Asia/Kolkata";
    public string  Currency  { get; set; } = "INR";
    public string? LogoUrl   { get; set; }
}
