using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Overtime;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class OTRuleConfiguration : IEntityTypeConfiguration<OTRule>
{
    public void Configure(EntityTypeBuilder<OTRule> builder)
    {
        builder.ToTable("OTRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.WeekdayMultiplier).HasColumnType("decimal(18,2)");
        builder.Property(r => r.WeekendMultiplier).HasColumnType("decimal(18,2)");
        builder.Property(r => r.HolidayMultiplier).HasColumnType("decimal(18,2)");
        builder.Property(r => r.RoundingMinutes).HasDefaultValue(0);
        builder.Property(r => r.DailyCapHours).HasDefaultValue(0);
        builder.Property(r => r.PayRunCapHours).HasDefaultValue(0);
        builder.Property(r => r.AppliesOnWeekend).HasDefaultValue(true);
        builder.Property(r => r.AppliesOnHoliday).HasDefaultValue(true);

        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);
    }
}
