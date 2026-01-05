using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayrollSettingsConfiguration : IEntityTypeConfiguration<PayrollSettings>
{
    public void Configure(EntityTypeBuilder<PayrollSettings> builder)
    {
        builder.ToTable("PayrollSettings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.WorkingDaysPerMonth).IsRequired();
        builder.Property(p => p.WorkingHoursPerDay).IsRequired();
        builder.Property(p => p.NoPayCalculationBasis)
            .IsRequired()
            .HasDefaultValue(PayrollSettingsDefaults.NoPayCalculationBasis);
        builder.Property(p => p.AttendanceHalfDayHours)
            .IsRequired()
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(PayrollSettingsDefaults.AttendanceHalfDayHours);
        builder.Property(p => p.WeekdayOvertimeMultiplier)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(PayrollSettingsDefaults.WeekdayOvertimeMultiplier);
        builder.Property(p => p.WeekendOvertimeMultiplier)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(PayrollSettingsDefaults.WeekendOvertimeMultiplier);
        builder.Property(p => p.HolidayOvertimeMultiplier)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(PayrollSettingsDefaults.HolidayOvertimeMultiplier);
        builder.Property(p => p.OvertimeRoundingMinutes)
            .IsRequired()
            .HasDefaultValue(PayrollSettingsDefaults.OvertimeRoundingMinutes);
        builder.Property(p => p.OvertimeDailyCapHours)
            .IsRequired()
            .HasDefaultValue(PayrollSettingsDefaults.OvertimeDailyCapHours);
        builder.Property(p => p.OvertimePayRunCapHours)
            .IsRequired()
            .HasDefaultValue(PayrollSettingsDefaults.OvertimePayRunCapHours);

        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.ModifiedBy).HasMaxLength(100);
    }
}
