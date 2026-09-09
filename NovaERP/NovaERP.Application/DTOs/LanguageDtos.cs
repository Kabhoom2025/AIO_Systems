namespace NovaERP.Application.DTOs;

public class LanguageDto
{
    public int    Id         { get; set; }
    public string Code       { get; set; } = string.Empty;
    public string Name       { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool   IsRtl      { get; set; }
    public bool   IsActive   { get; set; }
}
