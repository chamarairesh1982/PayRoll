using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class GeneratedTaxDocumentConfiguration : IEntityTypeConfiguration<GeneratedTaxDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedTaxDocument> builder)
    {
        builder.ToTable("GeneratedTaxDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName).HasMaxLength(256);
        builder.Property(d => d.FilePath).HasMaxLength(500);
        builder.Property(d => d.ContentType).HasMaxLength(100);
        builder.Property(d => d.GeneratedByUserId).HasMaxLength(100);
        builder.Property(d => d.GeneratedByUserName).HasMaxLength(200);
        builder.Property(d => d.ChecksumSha256).HasMaxLength(128);
        builder.Property(d => d.ErrorSummary).HasMaxLength(500);

        builder.HasOne(d => d.Employee)
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Company)
            .WithMany()
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Branch)
            .WithMany()
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.CostCenter)
            .WithMany()
            .HasForeignKey(d => d.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new
        {
            d.Type,
            d.Year,
            d.PeriodStart,
            d.PeriodEnd,
            d.EmployeeId,
            d.CompanyId,
            d.BranchId,
            d.CostCenterId
        });
    }
}
