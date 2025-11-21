using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PaySlipEarningLineConfiguration : IEntityTypeConfiguration<PaySlipEarningLine>
{
    public void Configure(EntityTypeBuilder<PaySlipEarningLine> builder)
    {
        builder.ToTable("PaySlipEarningLines");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.PaySlip)
            .WithMany(ps => ps.Earnings)
            .HasForeignKey(e => e.PaySlipId);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.ModifiedBy).HasMaxLength(100);
    }
}
