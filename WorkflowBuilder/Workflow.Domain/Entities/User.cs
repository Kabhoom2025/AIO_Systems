namespace Workflow.Domain.Entities;

public class User : BaseEntity
{
    public int     OrganizationId   { get; set; } = 1;
    public string  Name             { get; set; } = string.Empty;
    public string  Email            { get; set; } = string.Empty;
    public string  PasswordHash     { get; set; } = string.Empty;
    public string  Role             { get; set; } = "Admin";
    public bool    IsActive         { get; set; } = true;
    public DateTime? LastLoginAt    { get; set; }
}
