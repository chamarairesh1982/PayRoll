using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.Property(t => t.PayslipNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.PeriodDescription).HasMaxLength(100).IsRequired();
        builder.Property(t => t.PdfFilePath).HasMaxLength(500);

        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasIndex(t => new { t.TenantId, t.PayslipNumber }).IsUnique();
    }
}
