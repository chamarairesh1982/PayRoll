using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class RecurringPayItemRuleConfiguration : IEntityTypeConfiguration<RecurringPayItemRule>
{
    public void Configure(EntityTypeBuilder<RecurringPayItemRule> builder)
    {
        builder.ToTable("RecurringPayItemRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(r => r.AllowanceType)
            .WithMany()
            .HasForeignKey(r => r.AllowanceTypeId);

        builder.HasOne(r => r.DeductionType)
            .WithMany()
            .HasForeignKey(r => r.DeductionTypeId);

        builder.HasMany(r => r.Assignments)
            .WithOne(a => a.Rule)
            .HasForeignKey(a => a.RuleId);

        builder.HasCheckConstraint(
            "CK_RecurringPayItemRules_ComponentType",
            "((RuleType = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR (RuleType = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))");

        builder.HasCheckConstraint(
            "CK_RecurringPayItemRules_EffectiveDates",
            "([EndDate] IS NULL OR [EndDate] >= [StartDate])");
    }
}
