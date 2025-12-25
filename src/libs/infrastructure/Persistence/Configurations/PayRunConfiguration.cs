using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunConfiguration : IEntityTypeConfiguration<PayRun>
{
    public void Configure(EntityTypeBuilder<PayRun> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.TotalGross).HasPrecision(18, 2);
        builder.Property(t => t.TotalDeductions).HasPrecision(18, 2);
        builder.Property(t => t.TotalNet).HasPrecision(18, 2);

        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasMany(t => t.LineItems)
            .WithOne(t => t.PayRun)
            .HasForeignKey(t => t.PayRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
