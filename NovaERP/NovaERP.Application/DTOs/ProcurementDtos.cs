namespace NovaERP.Application.DTOs;

public class VendorDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Category     { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address      { get; set; }
    public bool    IsActive     { get; set; }
    public int     OwnerId      { get; set; }
    public string  OwnerName    { get; set; } = string.Empty;
}

public class CreateVendorDto
{
    public string  Name         { get; set; } = string.Empty;
    public string? Category     { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address      { get; set; }
    public bool    IsActive     { get; set; } = true;
    public int     OwnerId      { get; set; }
}

public class UpdateVendorDto
{
    public string  Name         { get; set; } = string.Empty;
    public string? Category     { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address      { get; set; }
    public bool    IsActive     { get; set; } = true;
    public int     OwnerId      { get; set; }
}

public class RfqItemDto
{
    public int     Id           { get; set; }
    public string  ItemName     { get; set; } = string.Empty;
    public decimal Quantity     { get; set; }
    public int     DisplayOrder { get; set; }
}

public class CreateRfqItemDto
{
    public string  ItemName     { get; set; } = string.Empty;
    public decimal Quantity     { get; set; }
    public int     DisplayOrder { get; set; }
}

public class RfqVendorQuoteDto
{
    public int       Id            { get; set; }
    public int       VendorId      { get; set; }
    public string    VendorName    { get; set; } = string.Empty;
    public decimal?  QuotedAmount  { get; set; }
    public string?   Notes         { get; set; }
    public DateTime? RespondedDate { get; set; }
}

public class RfqRequestDto
{
    public int       Id               { get; set; }
    public string    RfqNumber        { get; set; } = string.Empty;
    public string    Title            { get; set; } = string.Empty;
    public string?   Description      { get; set; }
    public string    Status           { get; set; } = string.Empty;
    public DateTime  IssueDate        { get; set; }
    public DateTime? ResponseDeadline { get; set; }
    public int       OwnerId          { get; set; }
    public string    OwnerName        { get; set; } = string.Empty;
    public int?      WinningVendorId  { get; set; }
    public string?   WinningVendorName { get; set; }
    public List<RfqItemDto> Items { get; set; } = new();
    public List<RfqVendorQuoteDto> Quotes { get; set; } = new();
}

public class CreateRfqRequestDto
{
    public string    Title            { get; set; } = string.Empty;
    public string?   Description      { get; set; }
    public DateTime  IssueDate        { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ResponseDeadline { get; set; }
    public int       OwnerId          { get; set; }
    public List<CreateRfqItemDto> Items { get; set; } = new();
    public List<int> InvitedVendorIds  { get; set; } = new();
}

/// <summary>Items and invited vendors are replaced wholesale, same "replace-all" convention
/// as TaxCode's Components and SalesOrder's Lines. Only permitted while Status is "Draft" —
/// enforced in the service, not here.</summary>
public class UpdateRfqRequestDto
{
    public string    Title            { get; set; } = string.Empty;
    public string?   Description      { get; set; }
    public DateTime  IssueDate        { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ResponseDeadline { get; set; }
    public int       OwnerId          { get; set; }
    public List<CreateRfqItemDto> Items { get; set; } = new();
    public List<int> InvitedVendorIds  { get; set; } = new();
}

public class RecordRfqQuoteDto
{
    public int      VendorId     { get; set; }
    public decimal  QuotedAmount { get; set; }
    public string?  Notes        { get; set; }
}

public class CloseRfqRequestDto
{
    public int? WinningVendorId { get; set; }
}
