using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppRecordConfiguration : IEntityTypeConfiguration<AppRecord>
{
    public void Configure(EntityTypeBuilder<AppRecord> builder)
    {
        builder.ToTable("app_records");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DataJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(r => r.AppDefinitionId);

        builder.HasOne(r => r.AppDefinition)
            .WithMany()
            .HasForeignKey(r => r.AppDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
