using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppNotificationRuleConfiguration : IEntityTypeConfiguration<AppNotificationRule>
{
    public void Configure(EntityTypeBuilder<AppNotificationRule> builder)
    {
        builder.ToTable("app_notification_rules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Subject).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Body).IsRequired();
        builder.Property(r => r.ConditionsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(r => r.RecipientRoleIdsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(r => r.RecipientUserIdsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(r => r.CcUserIdsJson).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(r => new { r.OrganizationId, r.AppId });
    }
}
