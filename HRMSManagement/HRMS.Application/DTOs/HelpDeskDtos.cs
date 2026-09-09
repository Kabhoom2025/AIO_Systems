namespace HRMS.Application.DTOs;

public class TicketDto
{
    public int       Id                 { get; set; }
    public string    TicketNumber       { get; set; } = string.Empty;
    public string    Category           { get; set; } = string.Empty;
    public string    Priority           { get; set; } = string.Empty;
    public string    Subject            { get; set; } = string.Empty;
    public string    Description        { get; set; } = string.Empty;
    public string    Status             { get; set; } = string.Empty;
    public int       RaisedByEmployeeId { get; set; }
    public string    RaisedByName       { get; set; } = string.Empty;
    public int?      AssignedToUserId   { get; set; }
    public string?   AssignedToName     { get; set; }
    public DateTime? SlaDueAt           { get; set; }
    public DateTime? ResolvedAt         { get; set; }
    public int       CommentCount       { get; set; }
    public DateTime  CreatedDate        { get; set; }
}

public class TicketCommentDto
{
    public int      Id           { get; set; }
    public int?     AuthorUserId { get; set; }
    public string   AuthorName   { get; set; } = string.Empty;
    public string   Comment      { get; set; } = string.Empty;
    public DateTime CreatedDate  { get; set; }
}

public class TicketDetailDto : TicketDto
{
    public List<TicketCommentDto> Comments { get; set; } = new();
}

public class CreateTicketDto
{
    public string Category    { get; set; } = "HR";
    public string Priority    { get; set; } = "Medium";
    public string Subject     { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AddCommentDto
{
    public string Comment { get; set; } = string.Empty;
}

public class AssignTicketDto
{
    public int UserId { get; set; }
}

public class UpdateTicketStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class HelpDeskSummaryDto
{
    public int Open        { get; set; }
    public int InProgress  { get; set; }
    public int Resolved    { get; set; }
    public int Closed      { get; set; }
    public int BreachedSla { get; set; }
}
