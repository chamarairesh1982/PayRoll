using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayslipDocumentConfiguration : IEntityTypeConfiguration<PayslipDocument>
{
    public void Configure(EntityTypeBuilder<PayslipDocument> builder)
    {
        builder.ToTable("PayslipDocuments");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.Status).HasConversion<int>();
        builder.Property(document => document.FileName).HasMaxLength(255);
        builder.Property(document => document.FilePath).HasMaxLength(500);
        builder.Property(document => document.ChecksumSha256).HasMaxLength(64);
        builder.Property(document => document.GeneratedByUserId).HasMaxLength(100);
        builder.Property(document => document.GeneratedByUserName).HasMaxLength(200);
        builder.Property(document => document.ErrorSummary).HasMaxLength(500);
        builder.Property(document => document.CreatedBy).HasMaxLength(100);
        builder.Property(document => document.ModifiedBy).HasMaxLength(100);

        builder.HasOne(document => document.PayRun)
            .WithMany(payRun => payRun.PayslipDocuments)
            .HasForeignKey(document => document.PayRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(document => document.Employee)
            .WithMany()
            .HasForeignKey(document => document.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(document => new { document.PayRunId, document.EmployeeId });
    }
}
