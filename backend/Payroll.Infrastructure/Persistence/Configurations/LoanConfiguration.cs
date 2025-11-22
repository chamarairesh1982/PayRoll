using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Loans;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.EmployeeId).IsRequired();
        builder.Property(l => l.PrincipalAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.OutstandingPrincipal).HasColumnType("decimal(18,2)");
        builder.Property(l => l.InstallmentAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Status).HasConversion<int>();

        builder.HasIndex(l => new { l.EmployeeId, l.Status });
    }
}
