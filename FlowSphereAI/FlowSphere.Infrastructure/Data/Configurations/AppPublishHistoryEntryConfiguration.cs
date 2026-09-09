using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppPublishHistoryEntryConfiguration : IEntityTypeConfiguration<AppPublishHistoryEntry>
{
    public void Configure(EntityTypeBuilder<AppPublishHistoryEntry> builder)
    {
        builder.ToTable("app_publish_history_entries");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PublishedByUserName).IsRequired().HasMaxLength(200);

        builder.HasIndex(p => new { p.OrganizationId, p.AppId });
    }
}
