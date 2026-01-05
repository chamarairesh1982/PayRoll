using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class RecurringRuleConfiguration : IEntityTypeConfiguration<RecurringRule>
{
    public void Configure(EntityTypeBuilder<RecurringRule> builder)
    {
        builder.Property(r => r.Code).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId);
    }
}
