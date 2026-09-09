using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class RegisteredServiceConfiguration : IEntityTypeConfiguration<RegisteredService>
{
    public void Configure(EntityTypeBuilder<RegisteredService> builder)
    {
        builder.ToTable("RegisteredServices");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RoutePrefix).IsRequired().HasMaxLength(50);
        builder.Property(s => s.BaseUrl).IsRequired().HasMaxLength(300);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.ModuleKeys).IsRequired().HasMaxLength(300);
        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.HealthCheckPath).HasMaxLength(100);
        builder.Property(s => s.CreatedDate).IsRequired();

        builder.Property(s => s.BackendWorkingDirectory).HasMaxLength(500);
        builder.Property(s => s.BackendCommand).HasMaxLength(300);
        builder.Property(s => s.FrontendUrl).HasMaxLength(300);
        builder.Property(s => s.FrontendWorkingDirectory).HasMaxLength(500);
        builder.Property(s => s.FrontendCommand).HasMaxLength(300);
        builder.Property(s => s.DockerServiceName).HasMaxLength(100);
        builder.Property(s => s.Icon).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Color).IsRequired().HasMaxLength(20);
        builder.Property(s => s.SortOrder).IsRequired();

        builder.HasIndex(s => s.RoutePrefix).IsUnique();
    }
}
