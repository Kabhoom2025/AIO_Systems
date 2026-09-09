namespace NovaERP.Application.DTOs;

public class LeadDto
{
    public int       Id                  { get; set; }
    public string    Name                { get; set; } = string.Empty;
    public string?   CompanyName         { get; set; }
    public string?   Email               { get; set; }
    public string?   Phone               { get; set; }
    public string?   Source              { get; set; }
    public string    Status              { get; set; } = string.Empty;
    public int       OwnerId             { get; set; }
    public string    OwnerName           { get; set; } = string.Empty;
    public int?      ConvertedAccountId  { get; set; }
    public string?   ConvertedAccountName { get; set; }
    public DateTime? ConvertedDate       { get; set; }
}

public class CreateLeadDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? Source      { get; set; }
    public int     OwnerId     { get; set; }
}

/// <summary>Status here excludes "Converted" — that transition only happens via
/// POST /api/leads/{id}/convert, never a plain field edit.</summary>
public class UpdateLeadDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? Source      { get; set; }
    public string  Status      { get; set; } = "New";
    public int     OwnerId     { get; set; }
}

public class ConvertLeadDto
{
    public string?  AccountName       { get; set; }
    public bool     CreateOpportunity { get; set; }
    public string?  OpportunityName   { get; set; }
    public decimal? OpportunityAmount { get; set; }
}

public class ConvertLeadResultDto
{
    public int  LeadId         { get; set; }
    public int  AccountId      { get; set; }
    public int  ContactId      { get; set; }
    public int? OpportunityId  { get; set; }
}

public class AccountDto
{
    public int     Id        { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string? Industry  { get; set; }
    public string? Website   { get; set; }
    public string? Phone     { get; set; }
    public int     OwnerId   { get; set; }
    public string  OwnerName { get; set; } = string.Empty;
}

public class CreateAccountDto
{
    public string  Name     { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Website  { get; set; }
    public string? Phone    { get; set; }
    public int     OwnerId  { get; set; }
}

public class UpdateAccountDto
{
    public string  Name     { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Website  { get; set; }
    public string? Phone    { get; set; }
    public int     OwnerId  { get; set; }
}

public class ContactDto
{
    public int     Id          { get; set; }
    public int?    AccountId   { get; set; }
    public string? AccountName { get; set; }
    public string  FirstName   { get; set; } = string.Empty;
    public string  LastName    { get; set; } = string.Empty;
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? Title       { get; set; }
    public int     OwnerId     { get; set; }
    public string  OwnerName   { get; set; } = string.Empty;
}

public class CreateContactDto
{
    public int?    AccountId { get; set; }
    public string  FirstName { get; set; } = string.Empty;
    public string  LastName  { get; set; } = string.Empty;
    public string? Email     { get; set; }
    public string? Phone     { get; set; }
    public string? Title     { get; set; }
    public int     OwnerId   { get; set; }
}

public class UpdateContactDto
{
    public int?    AccountId { get; set; }
    public string  FirstName { get; set; } = string.Empty;
    public string  LastName  { get; set; } = string.Empty;
    public string? Email     { get; set; }
    public string? Phone     { get; set; }
    public string? Title     { get; set; }
    public int     OwnerId   { get; set; }
}

public class OpportunityDto
{
    public int       Id          { get; set; }
    public int       AccountId   { get; set; }
    public string    AccountName { get; set; } = string.Empty;
    public string    Name        { get; set; } = string.Empty;
    public decimal   Amount      { get; set; }
    public string    Stage       { get; set; } = string.Empty;
    public DateTime? CloseDate   { get; set; }
    public int       OwnerId     { get; set; }
    public string    OwnerName   { get; set; } = string.Empty;
}

public class CreateOpportunityDto
{
    public int       AccountId { get; set; }
    public string    Name      { get; set; } = string.Empty;
    public decimal   Amount    { get; set; }
    public string    Stage     { get; set; } = "Qualification";
    public DateTime? CloseDate { get; set; }
    public int        OwnerId  { get; set; }
}

/// <summary>AccountId is immutable after creation — same "identifying field frozen on edit"
/// convention as ExchangeRate's currency pair / TaxCode's Code.</summary>
public class UpdateOpportunityDto
{
    public string    Name      { get; set; } = string.Empty;
    public decimal   Amount    { get; set; }
    public string    Stage     { get; set; } = "Qualification";
    public DateTime? CloseDate { get; set; }
    public int       OwnerId   { get; set; }
}
