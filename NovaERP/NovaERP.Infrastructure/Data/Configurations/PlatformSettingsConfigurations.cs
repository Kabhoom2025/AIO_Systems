using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class OrganizationSettingsConfiguration : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("OrganizationSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.DefaultLanguageCode).IsRequired().HasMaxLength(10);
        builder.Property(s => s.DefaultCurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(s => s.DefaultTimezone).IsRequired().HasMaxLength(100);
        builder.Property(s => s.DateFormat).IsRequired().HasMaxLength(20);
        builder.Property(s => s.TimeFormat).IsRequired().HasMaxLength(20);
        builder.Property(s => s.InvoiceNumberPrefix).IsRequired().HasMaxLength(20);
        builder.HasIndex(s => s.OrganizationId).IsUnique();

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FeatureToggleConfiguration : IEntityTypeConfiguration<FeatureToggle>
{
    public void Configure(EntityTypeBuilder<FeatureToggle> builder)
    {
        builder.ToTable("FeatureToggles");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.ModuleKey).IsRequired().HasMaxLength(50);
        builder.HasIndex(f => new { f.OrganizationId, f.ModuleKey }).IsUnique();

        builder.HasOne(f => f.Organization)
               .WithMany()
               .HasForeignKey(f => f.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Symbol).IsRequired().HasMaxLength(5);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FromCurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(r => r.ToCurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(r => r.Rate).HasColumnType("decimal(18,6)");
        builder.HasIndex(r => new { r.OrganizationId, r.FromCurrencyCode, r.ToCurrencyCode, r.EffectiveDate });

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Code).IsRequired().HasMaxLength(10);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.NativeName).IsRequired().HasMaxLength(100);
        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class OrganizationLanguageConfiguration : IEntityTypeConfiguration<OrganizationLanguage>
{
    public void Configure(EntityTypeBuilder<OrganizationLanguage> builder)
    {
        builder.ToTable("OrganizationLanguages");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.LanguageCode).IsRequired().HasMaxLength(10);
        builder.HasIndex(o => new { o.OrganizationId, o.LanguageCode }).IsUnique();

        builder.HasOne(o => o.Organization)
               .WithMany()
               .HasForeignKey(o => o.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FileName).IsRequired().HasMaxLength(300);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(150);
        builder.Property(d => d.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(d => d.EntityType).HasMaxLength(100);
        builder.HasIndex(d => new { d.OrganizationId, d.EntityType, d.EntityId });

        builder.HasOne(d => d.Organization)
               .WithMany()
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
