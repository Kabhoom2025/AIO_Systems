using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<AppDefinition>
{
    public void Configure(EntityTypeBuilder<AppDefinition> builder)
    {
        builder.ToTable("applications", "platform");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.HasIndex(a => a.Name).IsUnique();

        builder.HasMany(a => a.Screens).WithOne(s => s.Application!)
            .HasForeignKey(s => s.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.Tables).WithOne(t => t.Application!)
            .HasForeignKey(t => t.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.Apis).WithOne(x => x.Application!)
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.Services).WithOne(x => x.Application!)
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.Mappings).WithOne(x => x.Application!)
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.Versions).WithOne(x => x.Application!)
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationVersionConfiguration : IEntityTypeConfiguration<ApplicationVersion>
{
    public void Configure(EntityTypeBuilder<ApplicationVersion> builder)
    {
        builder.ToTable("application_versions", "platform");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(v => new { v.ApplicationId, v.VersionNumber }).IsUnique();
    }
}
