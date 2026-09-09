using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class ChatChannelConfiguration : IEntityTypeConfiguration<ChatChannel>
{
    public void Configure(EntityTypeBuilder<ChatChannel> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(c => new { c.OrganizationId, c.ProjectId });

        builder.HasOne(c => c.Organization).WithMany().HasForeignKey(c => c.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Project).WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.CreatedByUser).WithMany().HasForeignKey(c => c.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatChannelMemberConfiguration : IEntityTypeConfiguration<ChatChannelMember>
{
    public void Configure(EntityTypeBuilder<ChatChannelMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.ChannelId, m.UserId }).IsUnique();

        builder.HasOne(m => m.Channel).WithMany(c => c.Members).HasForeignKey(m => m.ChannelId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChannelReadConfiguration : IEntityTypeConfiguration<ChannelRead>
{
    public void Configure(EntityTypeBuilder<ChannelRead> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.ChannelId, r.UserId }).IsUnique();

        builder.HasOne(r => r.Channel).WithMany(c => c.Reads).HasForeignKey(r => r.ChannelId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DirectConversationConfiguration : IEntityTypeConfiguration<DirectConversation>
{
    public void Configure(EntityTypeBuilder<DirectConversation> builder)
    {
        builder.HasKey(d => d.Id);
        // UserAId is always normalized to the lexicographically-smaller Guid at creation time, so
        // this composite unique index guarantees (A,B) and (B,A) can never both exist.
        builder.HasIndex(d => new { d.UserAId, d.UserBId }).IsUnique();

        builder.HasOne(d => d.UserA).WithMany().HasForeignKey(d => d.UserAId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.UserB).WithMany().HasForeignKey(d => d.UserBId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConversationReadConfiguration : IEntityTypeConfiguration<ConversationRead>
{
    public void Configure(EntityTypeBuilder<ConversationRead> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.ConversationId, r.UserId }).IsUnique();

        builder.HasOne(r => r.Conversation).WithMany().HasForeignKey(r => r.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Body).IsRequired();
        builder.HasIndex(m => new { m.ChannelId, m.CreatedAt });
        builder.HasIndex(m => new { m.DirectConversationId, m.CreatedAt });

        builder.HasOne(m => m.Channel).WithMany(c => c.Messages).HasForeignKey(m => m.ChannelId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.DirectConversation).WithMany(d => d.Messages).HasForeignKey(m => m.DirectConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.AuthorUser).WithMany().HasForeignKey(m => m.AuthorUserId).OnDelete(DeleteBehavior.Restrict);

        // Self-referencing thread replies -> Restrict, mirrors WorkItem's ParentWorkItem pattern.
        builder.HasOne(m => m.ParentMessage).WithMany(m => m.Replies).HasForeignKey(m => m.ParentMessageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Emoji).IsRequired().HasMaxLength(16);
        builder.HasIndex(r => new { r.MessageId, r.UserId, r.Emoji }).IsUnique();

        builder.HasOne(r => r.Message).WithMany(m => m.Reactions).HasForeignKey(r => r.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(300);
        builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);

        builder.HasOne(a => a.Message).WithMany(m => m.Attachments).HasForeignKey(a => a.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.UploadedByUser).WithMany().HasForeignKey(a => a.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserPresenceConfiguration : IEntityTypeConfiguration<UserPresence>
{
    public void Configure(EntityTypeBuilder<UserPresence> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
