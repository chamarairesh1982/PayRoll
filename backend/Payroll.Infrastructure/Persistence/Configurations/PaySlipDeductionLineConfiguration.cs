using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PaySlipDeductionLineConfiguration : IEntityTypeConfiguration<PaySlipDeductionLine>
{
    public void Configure(EntityTypeBuilder<PaySlipDeductionLine> builder)
    {
        builder.ToTable("PaySlipDeductionLines");

        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.PaySlip)
            .WithMany(ps => ps.Deductions)
            .HasForeignKey(d => d.PaySlipId);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.ModifiedBy).HasMaxLength(100);
    }
}
