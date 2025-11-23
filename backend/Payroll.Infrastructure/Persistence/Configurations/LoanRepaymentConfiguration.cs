using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Loans;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class LoanRepaymentConfiguration : IEntityTypeConfiguration<LoanRepayment>
{
    public void Configure(EntityTypeBuilder<LoanRepayment> builder)
    {
        builder.ToTable("LoanRepayments");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DueDate)
            .IsRequired();

        builder.Property(r => r.IsPaid)
            .IsRequired();
    }
}
