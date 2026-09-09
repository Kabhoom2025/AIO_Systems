namespace AIO_Systems.DTOs.Organization;

public class LicenseDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int MaxUsers { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UpsertLicenseRequest
{
    public int OrganizationId { get; set; }
    public string Plan { get; set; } = "Basic";
    public string Status { get; set; } = "Trial";
    public DateTime ExpiryDate { get; set; }
    public int MaxUsers { get; set; } = 5;
    public string? Notes { get; set; }
}
