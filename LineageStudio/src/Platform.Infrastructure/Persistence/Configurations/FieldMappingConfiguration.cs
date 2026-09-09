using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class FieldMappingConfiguration : IEntityTypeConfiguration<FieldMapping>
{
    public void Configure(EntityTypeBuilder<FieldMapping> builder)
    {
        builder.ToTable("mappings", "platform");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.SourceField).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ApiField).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ServiceField).HasMaxLength(200);
        builder.Property(m => m.Transformation).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.TransformationConfigJson).HasColumnType("jsonb");

        builder.HasOne(m => m.SourceComponent).WithMany()
            .HasForeignKey(m => m.SourceComponentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Api).WithMany()
            .HasForeignKey(m => m.ApiId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Service).WithMany()
            .HasForeignKey(m => m.ServiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(m => m.Column).WithMany()
            .HasForeignKey(m => m.ColumnId).OnDelete(DeleteBehavior.Cascade);
    }
}
