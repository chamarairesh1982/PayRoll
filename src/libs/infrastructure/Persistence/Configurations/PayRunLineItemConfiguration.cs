using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunLineItemConfiguration : IEntityTypeConfiguration<PayRunLineItem>
{
    public void Configure(EntityTypeBuilder<PayRunLineItem> builder)
    {
        builder.Property(t => t.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(t => t.EmployeeName).HasMaxLength(200).IsRequired();

        builder.Property(t => t.BaseSalary).HasPrecision(18, 2);
        builder.Property(t => t.Allowances).HasPrecision(18, 2);
        builder.Property(t => t.Overtime).HasPrecision(18, 2);
        builder.Property(t => t.GrossAmount).HasPrecision(18, 2);
        builder.Property(t => t.EpfEmployee).HasPrecision(18, 2);
        builder.Property(t => t.EpfEmployer).HasPrecision(18, 2);
        builder.Property(t => t.Etf).HasPrecision(18, 2);
        builder.Property(t => t.Tax).HasPrecision(18, 2);
        builder.Property(t => t.OtherDeductions).HasPrecision(18, 2);
        builder.Property(t => t.TotalDeductions).HasPrecision(18, 2);
        builder.Property(t => t.NetAmount).HasPrecision(18, 2);

        builder.Property(t => t.RowVersion).IsRowVersion();
    }
}
