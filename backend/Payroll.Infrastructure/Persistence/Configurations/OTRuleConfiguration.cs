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

        builder.Property(r => r.Type).IsRequired().HasConversion<int>();
        builder.Property(r => r.Multiplier).HasColumnType("decimal(18,2)");
        builder.Property(r => r.RoundToMinutes).HasDefaultValue(15);
        builder.Property(r => r.RoundingMode)
            .HasConversion<int>()
            .HasDefaultValue(OvertimeRoundingMode.Nearest);
        builder.Property(r => r.DailyHoursCap).HasColumnType("float");
        builder.Property(r => r.MonthlyHoursCap).HasColumnType("float");
        builder.Property(r => r.EffectiveFrom).HasColumnType("date");
        builder.Property(r => r.EffectiveTo).HasColumnType("date");

        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);
    }
}
