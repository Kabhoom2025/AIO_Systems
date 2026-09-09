using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.ToTable("Dashboards");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(d => d.Organization)
               .WithMany()
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.User)
               .WithMany()
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("DashboardWidgets");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.WidgetType).IsRequired().HasMaxLength(50);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(100);
        builder.Property(w => w.SizeOption).IsRequired().HasMaxLength(20);

        builder.HasOne(w => w.Dashboard)
               .WithMany(d => d.Widgets)
               .HasForeignKey(w => w.DashboardId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
