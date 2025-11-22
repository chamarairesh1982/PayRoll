using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class DeductionLineConfiguration : IEntityTypeConfiguration<DeductionLine>
{
    public void Configure(EntityTypeBuilder<DeductionLine> builder)
    {
        builder.ToTable("DeductionLines");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne<PaySlip>()
            .WithMany(ps => ps.Deductions)
            .HasForeignKey(d => d.PaySlipId);
    }
}
