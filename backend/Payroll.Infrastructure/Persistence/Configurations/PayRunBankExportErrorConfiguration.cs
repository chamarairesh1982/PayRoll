using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunBankExportErrorConfiguration : IEntityTypeConfiguration<PayRunBankExportError>
{
    public void Configure(EntityTypeBuilder<PayRunBankExportError> builder)
    {
        builder.ToTable("PayRunBankExportErrors");
        builder.HasKey(error => error.Id);
        builder.Property(error => error.EmployeeCode).HasMaxLength(50);
        builder.Property(error => error.Field).HasMaxLength(100).IsRequired();
        builder.Property(error => error.Message).HasMaxLength(500).IsRequired();
        builder.Property(error => error.CreatedBy).HasMaxLength(100);
        builder.Property(error => error.ModifiedBy).HasMaxLength(100);

        builder.HasOne(error => error.PayRunBankExport)
            .WithMany(export => export.Errors)
            .HasForeignKey(error => error.PayRunBankExportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(error => error.PayRunBankExportId);
    }
}
