using Chatbot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Language).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Voice).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Theme).IsRequired().HasMaxLength(20);
        builder.Property(s => s.AiProvider).HasMaxLength(30);
        builder.Property(s => s.AiModel).HasMaxLength(200);

        builder.HasIndex(s => s.UserId).IsUnique();
    }
}
