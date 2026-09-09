using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppOtpChallengeConfiguration : IEntityTypeConfiguration<AppOtpChallenge>
{
    public void Configure(EntityTypeBuilder<AppOtpChallenge> builder)
    {
        builder.ToTable("app_otp_challenges");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Recipient).IsRequired().HasMaxLength(320);
        builder.Property(c => c.CodeHash).IsRequired().HasMaxLength(200);

        builder.HasIndex(c => new { c.AppId, c.Channel, c.Recipient });
    }
}
