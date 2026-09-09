namespace HRMS.Application.DTOs;

public class NotificationDto
{
    public int      Id          { get; set; }
    public int?     UserId      { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public string   Message     { get; set; } = string.Empty;
    public string   Type        { get; set; } = "Info";
    public string?  Link        { get; set; }
    public bool     IsRead      { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateNotificationDto
{
    public int?    UserId  { get; set; } // null = broadcast to org
    public string  Title   { get; set; } = string.Empty;
    public string  Message { get; set; } = string.Empty;
    public string  Type    { get; set; } = "Info";
    public string? Link    { get; set; }
}
