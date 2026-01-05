using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class RecurringPayItemAssignmentConfiguration : IEntityTypeConfiguration<RecurringPayItemAssignment>
{
    public void Configure(EntityTypeBuilder<RecurringPayItemAssignment> builder)
    {
        builder.ToTable("RecurringPayItemAssignments");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId);

        builder.HasIndex(a => new { a.RuleId, a.EmployeeId });

        builder.HasCheckConstraint(
            "CK_RecurringPayItemAssignments_EffectiveDates",
            "([EndDate] IS NULL OR [EndDate] >= [StartDate])");
    }
}
