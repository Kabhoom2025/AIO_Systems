namespace NovaERP.Application.DTOs;

/// <summary>The caller's own Vendor record — a portal-specific projection, same "my data"
/// framing as CustomerPortalService's MyContactDto.</summary>
public class MyVendorDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Category     { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address      { get; set; }
    public bool    IsActive     { get; set; }
}
