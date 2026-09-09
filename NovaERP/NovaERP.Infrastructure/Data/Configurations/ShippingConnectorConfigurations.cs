using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ShippingConnectorConfiguration : IEntityTypeConfiguration<ShippingConnector>
{
    public void Configure(EntityTypeBuilder<ShippingConnector> builder)
    {
        builder.ToTable("ShippingConnectors");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.TmsType).HasMaxLength(100);
        builder.Property(c => c.BaseUrl).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.HttpMethod).IsRequired().HasMaxLength(10);
        builder.Property(c => c.RequestContentType).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ResponseFormat).IsRequired().HasMaxLength(20);
        builder.Property(c => c.AuthType).IsRequired().HasMaxLength(20);
        builder.Property(c => c.AuthHeaderName).HasMaxLength(200);
        builder.Property(c => c.AuthApiKey).HasMaxLength(1000);
        builder.Property(c => c.AuthUsername).HasMaxLength(200);
        builder.Property(c => c.AuthPassword).HasMaxLength(1000);
        builder.Property(c => c.AuthTokenUrl).HasMaxLength(1000);
        builder.Property(c => c.AuthTokenResponsePath).HasMaxLength(200);
        builder.HasIndex(c => c.OrganizationId);

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShippingConnectorFieldMappingConfiguration : IEntityTypeConfiguration<ShippingConnectorFieldMapping>
{
    public void Configure(EntityTypeBuilder<ShippingConnectorFieldMapping> builder)
    {
        builder.ToTable("ShippingConnectorFieldMappings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Direction).IsRequired().HasMaxLength(20);
        builder.Property(m => m.NovaField).IsRequired().HasMaxLength(200);
        builder.Property(m => m.ExternalPath).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Transform).IsRequired().HasMaxLength(20);
        builder.Property(m => m.ConstantValue).HasMaxLength(500);

        builder.HasOne(m => m.Connector)
               .WithMany(c => c.FieldMappings)
               .HasForeignKey(m => m.ConnectorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
