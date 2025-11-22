using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Attendance;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId).IsRequired();

        builder.OwnsOne(a => a.Period, b =>
        {
            b.Property(p => p.Start).HasColumnName("PeriodStart");
            b.Property(p => p.End).HasColumnName("PeriodEnd");
        });

        builder.HasIndex("EmployeeId", "PeriodStart", "PeriodEnd");
    }
}
