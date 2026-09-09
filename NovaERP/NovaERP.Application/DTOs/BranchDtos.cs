namespace NovaERP.Application.DTOs;

public class BranchDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? Address      { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public string? Email        { get; set; }
    public string  Timezone     { get; set; } = "Asia/Kolkata";
    public bool    IsHeadOffice { get; set; }
    public bool    IsActive     { get; set; }
}

public class CreateBranchDto
{
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? Address      { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public string? Email        { get; set; }
    public string  Timezone     { get; set; } = "Asia/Kolkata";
    public bool    IsHeadOffice { get; set; }
}

public class UpdateBranchDto
{
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? Address      { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public string? Email        { get; set; }
    public string  Timezone     { get; set; } = "Asia/Kolkata";
    public bool    IsHeadOffice { get; set; }
    public bool    IsActive     { get; set; }
}
