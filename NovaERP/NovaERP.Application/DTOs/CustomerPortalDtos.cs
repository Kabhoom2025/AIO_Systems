namespace NovaERP.Application.DTOs;

/// <summary>The caller's own Contact record plus the linked Account's name — a portal-specific
/// projection, same "my data" framing as EmployeePortalService's use of EmployeeDto.</summary>
public class MyContactDto
{
    public int     Id          { get; set; }
    public string  FirstName   { get; set; } = string.Empty;
    public string  LastName    { get; set; } = string.Empty;
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? Title       { get; set; }
    public int?    AccountId   { get; set; }
    public string? AccountName { get; set; }
}
