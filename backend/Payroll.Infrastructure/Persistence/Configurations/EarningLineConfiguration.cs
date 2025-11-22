using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EarningLineConfiguration : IEntityTypeConfiguration<EarningLine>
{
    public void Configure(EntityTypeBuilder<EarningLine> builder)
    {
        builder.ToTable("EarningLines");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne<PaySlip>()
            .WithMany(ps => ps.Earnings)
            .HasForeignKey(e => e.PaySlipId);
    }
}
