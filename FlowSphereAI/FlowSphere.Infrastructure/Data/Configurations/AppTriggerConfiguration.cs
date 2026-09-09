using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppTriggerConfiguration : IEntityTypeConfiguration<AppTrigger>
{
    public void Configure(EntityTypeBuilder<AppTrigger> builder)
    {
        builder.ToTable("app_triggers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.FieldMappingJson).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(t => new { t.OrganizationId, t.SourceAppId });
    }
}
