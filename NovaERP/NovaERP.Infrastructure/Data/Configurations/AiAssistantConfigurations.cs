using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(150);

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Role).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.PendingActionType).HasMaxLength(50);
        builder.Property(m => m.PendingActionStatus).HasMaxLength(20);

        builder.HasOne(m => m.Conversation)
               .WithMany(c => c.Messages)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiAssistantSettingsConfiguration : IEntityTypeConfiguration<AiAssistantSettings>
{
    public void Configure(EntityTypeBuilder<AiAssistantSettings> builder)
    {
        builder.ToTable("AiAssistantSettings");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.OrganizationId).IsUnique();

        builder.Property(s => s.Provider).IsRequired().HasMaxLength(20);
        builder.Property(s => s.AnthropicApiKey).HasMaxLength(500);
        builder.Property(s => s.AnthropicModel).HasMaxLength(100);
        builder.Property(s => s.GroqApiKey).HasMaxLength(500);
        builder.Property(s => s.GroqModel).HasMaxLength(100);

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
