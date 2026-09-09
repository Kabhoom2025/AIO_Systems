namespace ProjectFlowAI.Domain.Entities;

/// <summary>A chat channel. ProjectId null means an org-wide channel; set means it's scoped to one Project.</summary>
public class ChatChannel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
    public Project? Project { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<ChatChannelMember> Members { get; set; } = new List<ChatChannelMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChannelRead> Reads { get; set; } = new List<ChannelRead>();
}

public class ChatChannelMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChannelId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ChatChannel? Channel { get; set; }
    public User? User { get; set; }
}

/// <summary>Per-user "read up to" marker for a channel — drives ChatChannelDto.UnreadCount.</summary>
public class ChannelRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChannelId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public ChatChannel? Channel { get; set; }
    public User? User { get; set; }
}

/// <summary>One 1:1 conversation between two users. UserAId is always the lexicographically-smaller
/// of the two Guids at creation time, so (A,B) and (B,A) never create two rows — see
/// CreateConversationCommandHandler for the normalization logic and its unit test.</summary>
public class DirectConversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? UserA { get; set; }
    public User? UserB { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

/// <summary>Per-user "read up to" marker for a DM conversation — parallel to ChannelRead, kept as a
/// separate entity (rather than reusing ChannelRead) since its FK targets DirectConversation, not
/// ChatChannel. Updated automatically whenever the current user fetches a conversation's messages
/// (there's no separate "mark conversation read" endpoint in this phase's contract).</summary>
public class ConversationRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public DirectConversation? Conversation { get; set; }
    public User? User { get; set; }
}

/// <summary>Exactly one of ChannelId/DirectConversationId is set — enforced in the command handler, not the DB.</summary>
public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ChannelId { get; set; }
    public Guid? DirectConversationId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid? ParentMessageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ChatChannel? Channel { get; set; }
    public DirectConversation? DirectConversation { get; set; }
    public User? AuthorUser { get; set; }
    public ChatMessage? ParentMessage { get; set; }
    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}

public class MessageReaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;

    public ChatMessage? Message { get; set; }
    public User? User { get; set; }
}

public class MessageAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatMessage? Message { get; set; }
    public User? UploadedByUser { get; set; }
}

/// <summary>One row per user, tracking live online/offline state as driven by ChatHub connect/disconnect.</summary>
public class UserPresence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public User? User { get; set; }
}
