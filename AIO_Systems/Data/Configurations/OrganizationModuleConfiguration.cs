using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class OrganizationModuleConfiguration : IEntityTypeConfiguration<OrganizationModule>
{
    public void Configure(EntityTypeBuilder<OrganizationModule> builder)
    {
        builder.ToTable("OrganizationModules");

        builder.HasKey(om => new { om.OrganizationId, om.PlatformModuleId });

        builder.Property(om => om.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasOne(om => om.Organization)
            .WithMany()
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(om => om.PlatformModule)
            .WithMany(p => p.OrganizationModules)
            .HasForeignKey(om => om.PlatformModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
