using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunRecurringLineConfiguration : IEntityTypeConfiguration<PayRunRecurringLine>
{
    public void Configure(EntityTypeBuilder<PayRunRecurringLine> builder)
    {
        builder.ToTable("PayRunRecurringLines");
        builder.HasKey(r => r.Id);

        builder.HasOne<PayRun>()
            .WithMany()
            .HasForeignKey(r => r.PayRunId);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(r => r.EmployeeId);

        builder.HasOne<RecurringPayItemRule>()
            .WithMany()
            .HasForeignKey(r => r.RuleId);

        builder.HasIndex(r => new { r.PayRunId, r.EmployeeId, r.RuleId })
            .IsUnique();
    }
}
