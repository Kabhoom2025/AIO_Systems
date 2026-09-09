namespace NovaERP.Application.DTOs;

public class OrganizationLanguageDto
{
    public int    Id           { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public bool   IsDefault    { get; set; }
}

/// <summary>Replaces the full set of languages an org supports in one call — simpler for the
/// UI than incremental add/remove, and keeps "exactly one default" easy to enforce.</summary>
public class UpdateOrganizationLanguagesDto
{
    public List<string> LanguageCodes      { get; set; } = new();
    public string        DefaultLanguageCode { get; set; } = string.Empty;
}
