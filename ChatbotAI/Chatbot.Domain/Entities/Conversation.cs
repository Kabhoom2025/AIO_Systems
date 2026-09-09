using Chatbot.Domain.Common;

namespace Chatbot.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = "New Chat";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
