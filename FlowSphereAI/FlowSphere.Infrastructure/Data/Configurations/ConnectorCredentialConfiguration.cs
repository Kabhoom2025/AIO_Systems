using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class ConnectorCredentialConfiguration : IEntityTypeConfiguration<ConnectorCredential>
{
    public void Configure(EntityTypeBuilder<ConnectorCredential> builder)
    {
        builder.ToTable("connector_credentials");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.KeyName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.EncryptedValue).IsRequired();

        builder.HasIndex(c => new { c.OrganizationId, c.ConnectorId });

        builder.HasOne(c => c.Connector)
            .WithMany(conn => conn.Credentials)
            .HasForeignKey(c => c.ConnectorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
