using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PaySlipConfiguration : IEntityTypeConfiguration<PaySlip>
{
    public void Configure(EntityTypeBuilder<PaySlip> builder)
    {
        builder.ToTable("PaySlips");

        builder.HasKey(ps => ps.Id);

        builder.HasOne(ps => ps.PayRun)
            .WithMany(pr => pr.PaySlips)
            .HasForeignKey(ps => ps.PayRunId);

        builder.HasIndex(ps => new { ps.PayRunId, ps.EmployeeId });

        builder.Property(ps => ps.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.TotalEarnings).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.NetPay).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployeeEpf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployerEpf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployerEtf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.PayeTax).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(ps => ps.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ps => ps.CreatedBy).HasMaxLength(100);
        builder.Property(ps => ps.ModifiedBy).HasMaxLength(100);
    }
}
