using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunBankExportConfiguration : IEntityTypeConfiguration<PayRunBankExport>
{
    public void Configure(EntityTypeBuilder<PayRunBankExport> builder)
    {
        builder.ToTable("PayRunBankExports");
        builder.HasKey(export => export.Id);
        builder.Property(export => export.Status).HasConversion<int>();
        builder.Property(export => export.FileName).HasMaxLength(255);
        builder.Property(export => export.FilePath).HasMaxLength(500);
        builder.Property(export => export.ChecksumSha256).HasMaxLength(64);
        builder.Property(export => export.GeneratedByUserId).HasMaxLength(100);
        builder.Property(export => export.GeneratedByUserName).HasMaxLength(200);
        builder.Property(export => export.DownloadedByUserId).HasMaxLength(100);
        builder.Property(export => export.DownloadedByUserName).HasMaxLength(200);
        builder.Property(export => export.ErrorSummary).HasMaxLength(500);
        builder.Property(export => export.CreatedBy).HasMaxLength(100);
        builder.Property(export => export.ModifiedBy).HasMaxLength(100);

        builder.HasOne(export => export.PayRun)
            .WithMany(payRun => payRun.BankExports)
            .HasForeignKey(export => export.PayRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(export => export.Template)
            .WithMany()
            .HasForeignKey(export => export.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(export => new { export.PayRunId, export.TemplateId });
    }
}
