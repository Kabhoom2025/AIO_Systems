namespace HRMS.Domain.Entities;

public class HelpDeskTicket : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public int      RaisedByEmployeeId { get; set; }
    public string   TicketNumber     { get; set; } = string.Empty; // e.g. TCK-0001
    public string   Category         { get; set; } = "HR"; // HR | IT | Finance | Admin
    public string   Priority         { get; set; } = "Medium"; // Low | Medium | High | Critical
    public string   Subject          { get; set; } = string.Empty;
    public string   Description      { get; set; } = string.Empty;
    public string   Status           { get; set; } = "Open"; // Open | InProgress | Resolved | Closed
    public int?     AssignedToUserId { get; set; }
    public DateTime? SlaDueAt        { get; set; }
    public DateTime? ResolvedAt      { get; set; }

    public Organization Organization     { get; set; } = null!;
    public Employee     RaisedByEmployee { get; set; } = null!;
    public User?        AssignedToUser   { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
}

public class TicketComment : BaseEntity
{
    public int    TicketId     { get; set; }
    public int?   AuthorUserId { get; set; }
    public string AuthorName   { get; set; } = string.Empty;
    public string Comment      { get; set; } = string.Empty;

    public HelpDeskTicket Ticket     { get; set; } = null!;
    public User?          AuthorUser { get; set; }
}
