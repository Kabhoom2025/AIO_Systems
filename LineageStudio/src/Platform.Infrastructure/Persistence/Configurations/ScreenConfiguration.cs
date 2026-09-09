using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

public class ScreenConfiguration : IEntityTypeConfiguration<Screen>
{
    public void Configure(EntityTypeBuilder<Screen> builder)
    {
        builder.ToTable("screens", "platform");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Route).HasMaxLength(500).IsRequired();
        builder.HasIndex(s => new { s.ApplicationId, s.Route }).IsUnique();

        builder.HasMany(s => s.Components).WithOne(c => c.Screen!)
            .HasForeignKey(c => c.ScreenId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UiComponentConfiguration : IEntityTypeConfiguration<UiComponent>
{
    public void Configure(EntityTypeBuilder<UiComponent> builder)
    {
        builder.ToTable("components", "platform");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.PropertiesJson).HasColumnType("jsonb");
        builder.Property(c => c.ValidationJson).HasColumnType("jsonb");
        builder.Property(c => c.EventsJson).HasColumnType("jsonb");
        builder.Property(c => c.DataBinding).HasMaxLength(200);
    }
}
