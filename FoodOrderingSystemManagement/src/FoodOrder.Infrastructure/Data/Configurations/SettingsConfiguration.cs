using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class SettingsConfiguration : IEntityTypeConfiguration<Settings>
{
    public void Configure(EntityTypeBuilder<Settings> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TaxPercentage)
            .HasColumnType("numeric(5,2)")
            .HasDefaultValue(0m);

        builder.Property(s => s.RestaurantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.GSTNumber)
            .HasMaxLength(50);

        builder.Property(s => s.LogoUrl);  // nullable TEXT — stores base64 data URL

        builder.Property(s => s.PointsPerRupee)
            .HasColumnType("numeric(5,2)")
            .HasDefaultValue(1.0m);

        builder.Property(s => s.PointsRedeemRate)
            .HasColumnType("numeric(5,2)")
            .HasDefaultValue(0.25m);

        // ── POS Settings ──────────────────────────────────────────────────────
        builder.Property(s => s.PosDefaultOrderType)
            .HasMaxLength(50)
            .HasDefaultValue("DineIn");

        builder.Property(s => s.PosAutoPrintBill)
            .HasDefaultValue(false);

        builder.Property(s => s.PosEnableTips)
            .HasDefaultValue(false);

        builder.Property(s => s.PosRequireCustomerPhone)
            .HasDefaultValue(false);

        // ── Menu Settings ─────────────────────────────────────────────────────
        builder.Property(s => s.MenuShowImages)
            .HasDefaultValue(true);

        builder.Property(s => s.MenuShowDescriptions)
            .HasDefaultValue(true);

        builder.Property(s => s.MenuShowOutOfStock)
            .HasDefaultValue(true);

        // ── Tax Configuration (extended) ──────────────────────────────────────
        builder.Property(s => s.TaxName)
            .HasMaxLength(50)
            .HasDefaultValue("GST");

        builder.Property(s => s.TaxInclusive)
            .HasDefaultValue(false);

        // ── Printer Settings ──────────────────────────────────────────────────
        builder.Property(s => s.PrinterEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.PrinterType)
            .HasMaxLength(50)
            .HasDefaultValue("Thermal");

        builder.Property(s => s.PrinterIp)
            .HasMaxLength(100);

        builder.Property(s => s.PrinterPort)
            .HasDefaultValue(9100);

        builder.Property(s => s.PrinterAutoPrint)
            .HasDefaultValue(false);

        // ── Payment Gateway (extended) ────────────────────────────────────────
        builder.Property(s => s.RazorpayKeyId)
            .HasMaxLength(200);

        builder.Property(s => s.RazorpayKeySecret)
            .HasMaxLength(200);

        builder.Property(s => s.PaymentCashEnabled)
            .HasDefaultValue(true);

        builder.Property(s => s.PaymentCardEnabled)
            .HasDefaultValue(true);

        builder.Property(s => s.PaymentOnlineEnabled)
            .HasDefaultValue(false);

        // ── Email Configuration ───────────────────────────────────────────────
        builder.Property(s => s.SmtpHost)
            .HasMaxLength(200);

        builder.Property(s => s.SmtpPort)
            .HasDefaultValue(587);

        builder.Property(s => s.SmtpUsername)
            .HasMaxLength(200);

        builder.Property(s => s.SmtpPassword)
            .HasMaxLength(200);

        builder.Property(s => s.SmtpFromEmail)
            .HasMaxLength(200);

        builder.Property(s => s.SmtpFromName)
            .HasMaxLength(200);

        builder.Property(s => s.SmtpSsl)
            .HasDefaultValue(true);

        // ── Backup Settings ───────────────────────────────────────────────────
        builder.Property(s => s.AutoBackupEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.BackupEmail)
            .HasMaxLength(200);

        builder.Property(s => s.BackupFrequencyHours)
            .HasDefaultValue(24);

        // ── Theme Settings ────────────────────────────────────────────────────
        builder.Property(s => s.ThemePrimaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#bf360c");

        builder.Property(s => s.ThemeMode)
            .HasMaxLength(20)
            .HasDefaultValue("light");

        // Seed a default settings record — the app always expects exactly one row (Id = 1).
        builder.HasData(new Settings
        {
            Id = 1,
            TaxPercentage = 5m,
            RestaurantName = "My Restaurant",
            Address = "123 Main Street",
            Phone = "000-000-0000",
            GSTNumber = "GST000000",
            PointsPerRupee = 1.0m,
            PointsRedeemRate = 0.25m,
            PromoAutoScroll = true,

            // POS
            PosDefaultOrderType     = "DineIn",
            PosAutoPrintBill        = false,
            PosEnableTips           = false,
            PosRequireCustomerPhone = false,

            // Menu
            MenuShowImages       = true,
            MenuShowDescriptions = true,
            MenuShowOutOfStock   = true,

            // Tax (extended)
            TaxName      = "GST",
            TaxInclusive = false,

            // Printer
            PrinterEnabled   = false,
            PrinterType      = "Thermal",
            PrinterPort      = 9100,
            PrinterAutoPrint = false,

            // Payment
            PaymentCashEnabled   = true,
            PaymentCardEnabled   = true,
            PaymentOnlineEnabled = false,

            // Email
            SmtpPort = 587,
            SmtpSsl  = true,

            // Backup
            AutoBackupEnabled    = false,
            BackupFrequencyHours = 24,

            // Theme
            ThemePrimaryColor = "#bf360c",
            ThemeMode         = "light",
        });
    }
}
