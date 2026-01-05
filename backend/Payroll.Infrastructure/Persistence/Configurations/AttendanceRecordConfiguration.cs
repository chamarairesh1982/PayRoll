using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Attendance;
using Payroll.Domain.ValueObjects;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.HoursWorked)
            .HasColumnType("decimal(18,2)");

        // Map the DateRange value object as an owned type
        builder.OwnsOne(x => x.Period, period =>
        {
            period.Property(p => p.Start)
                .HasColumnName("PeriodStart");

            period.Property(p => p.End)
                .HasColumnName("PeriodEnd");
        });
    }
}
