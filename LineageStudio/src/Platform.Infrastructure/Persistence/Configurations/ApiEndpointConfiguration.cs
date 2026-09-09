using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class ApiEndpointConfiguration : IEntityTypeConfiguration<ApiEndpoint>
{
    public void Configure(EntityTypeBuilder<ApiEndpoint> builder)
    {
        builder.ToTable("apis", "platform");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Method).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.Path).HasMaxLength(500).IsRequired();
        builder.Property(a => a.RequestSchemaJson).HasColumnType("jsonb");
        builder.Property(a => a.ResponseSchemaJson).HasColumnType("jsonb");
        builder.HasIndex(a => new { a.ApplicationId, a.Method, a.Path }).IsUnique();

        builder.HasOne(a => a.Service).WithMany()
            .HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Table).WithMany()
            .HasForeignKey(a => a.TableId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceDefinitionConfiguration : IEntityTypeConfiguration<ServiceDefinition>
{
    public void Configure(EntityTypeBuilder<ServiceDefinition> builder)
    {
        builder.ToTable("services", "platform");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.HasIndex(s => new { s.ApplicationId, s.Name }).IsUnique();

        builder.HasOne(s => s.Table).WithMany()
            .HasForeignKey(s => s.TableId).OnDelete(DeleteBehavior.SetNull);
    }
}
