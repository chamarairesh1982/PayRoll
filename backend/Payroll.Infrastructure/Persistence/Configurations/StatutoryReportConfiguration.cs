using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class StatutoryReportConfiguration : IEntityTypeConfiguration<StatutoryReport>
{
    public void Configure(EntityTypeBuilder<StatutoryReport> builder)
    {
        builder.ToTable("StatutoryReports");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.GeneratedBy).HasMaxLength(200);
        builder.Property(r => r.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(r => r.FileName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Checksum).HasMaxLength(128).IsRequired();
        builder.Property(r => r.WarningFilePath).HasMaxLength(500);
        builder.Property(r => r.WarningFileName).HasMaxLength(200);
        builder.Property(r => r.WarningContentType).HasMaxLength(100);

        builder.HasIndex(r => new { r.PayRunId, r.Type });
        builder.HasIndex(r => r.GeneratedAtUtc);
    }
}
