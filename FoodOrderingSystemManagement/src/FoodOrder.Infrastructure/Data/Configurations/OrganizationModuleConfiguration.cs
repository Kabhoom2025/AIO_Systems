using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class OrganizationModuleConfiguration : IEntityTypeConfiguration<OrganizationModule>
{
    public void Configure(EntityTypeBuilder<OrganizationModule> builder)
    {
        builder.ToTable("OrganizationModules");
        builder.HasKey(om => new { om.OrganizationId, om.PlatformModuleId });

        builder.HasOne(om => om.Organization)
               .WithMany()
               .HasForeignKey(om => om.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(om => om.PlatformModule)
               .WithMany(pm => pm.OrganizationModules)
               .HasForeignKey(om => om.PlatformModuleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
