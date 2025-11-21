using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Overtime;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class OvertimeRecordConfiguration : IEntityTypeConfiguration<OvertimeRecord>
{
    public void Configure(EntityTypeBuilder<OvertimeRecord> builder)
    {
        builder.ToTable("OvertimeRecords");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.EmployeeId).IsRequired();
        builder.Property(o => o.Date).IsRequired().HasColumnType("date");
        builder.Property(o => o.Hours).IsRequired();
        builder.Property(o => o.Type).IsRequired().HasConversion<int>();
        builder.Property(o => o.Status).IsRequired().HasConversion<int>().HasDefaultValue(OvertimeStatus.Pending);
        builder.Property(o => o.IsLockedForPayroll).HasDefaultValue(false);

        builder.Property(o => o.Reason).HasMaxLength(500);
        builder.Property(o => o.CreatedBy).HasMaxLength(100);
        builder.Property(o => o.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(o => new { o.EmployeeId, o.Date });
    }
}
