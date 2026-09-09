using FlowSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowSphere.Infrastructure.Data.Configurations;

public class AppRecordCommentConfiguration : IEntityTypeConfiguration<AppRecordComment>
{
    public void Configure(EntityTypeBuilder<AppRecordComment> builder)
    {
        builder.ToTable("app_record_comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Text).IsRequired().HasMaxLength(4000);

        builder.HasIndex(c => c.AppRecordId);
    }
}
