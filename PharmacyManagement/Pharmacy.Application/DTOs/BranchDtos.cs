namespace Pharmacy.Application.DTOs;

public class BranchDto
{
    public int     Id      { get; set; }
    public string  Name    { get; set; } = string.Empty;
    public string  Code    { get; set; } = string.Empty;
    public string  Type    { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone   { get; set; }
    public string? Email   { get; set; }
    public bool    IsActive { get; set; }
}

public class CreateBranchDto
{
    public string  Name    { get; set; } = string.Empty;
    public string  Code    { get; set; } = string.Empty;
    public string  Type    { get; set; } = "Retail";
    public string? Address { get; set; }
    public string? Phone   { get; set; }
    public string? Email   { get; set; }
}

public class UpdateBranchDto : CreateBranchDto
{
    public bool IsActive { get; set; } = true;
}

public class PublicBranchDto
{
    public int     Id      { get; set; }
    public string  Name    { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone   { get; set; }
    public string? Email   { get; set; }
}
