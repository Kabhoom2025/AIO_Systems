namespace AIO_Systems.DTOs.Organization;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Timezone { get; set; }
    public string? Currency { get; set; }
    public string TenantKey { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
    public string? Currency { get; set; } = "INR";
}

public class UpdateOrganizationRequest : CreateOrganizationRequest
{
    public bool IsActive { get; set; } = true;
}

public class OrgUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateOrgAdminRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
