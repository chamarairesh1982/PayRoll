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

        builder.Property(ps => ps.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.TotalEarnings).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.NetPay).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployeeEpf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployerEpf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.EmployerEtf).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.PayeTax).HasColumnType("decimal(18,2)");

        builder.HasOne(ps => ps.PayRun)
            .WithMany(pr => pr.PaySlips)
            .HasForeignKey(ps => ps.PayRunId);

        builder.HasOne(ps => ps.Employee)
            .WithMany()
            .HasForeignKey(ps => ps.EmployeeId);

        builder.HasMany(ps => ps.Earnings)
            .WithOne()
            .HasForeignKey(e => e.PaySlipId);

        builder.HasMany(ps => ps.Deductions)
            .WithOne()
            .HasForeignKey(d => d.PaySlipId);
    }
}
