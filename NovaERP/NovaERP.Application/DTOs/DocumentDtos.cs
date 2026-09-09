namespace NovaERP.Application.DTOs;

public class DocumentDto
{
    public int      Id               { get; set; }
    public string   FileName         { get; set; } = string.Empty;
    public string   ContentType      { get; set; } = string.Empty;
    public long     SizeBytes        { get; set; }
    public string?  EntityType       { get; set; }
    public int?     EntityId         { get; set; }
    public int      UploadedByUserId { get; set; }
    public DateTime UploadedDate     { get; set; }
}
