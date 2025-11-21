using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Leave;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.EmployeeId).IsRequired();
        builder.Property(l => l.LeaveType).IsRequired().HasConversion<int>();
        builder.Property(l => l.StartDate).IsRequired().HasColumnType("date");
        builder.Property(l => l.EndDate).IsRequired().HasColumnType("date");
        builder.Property(l => l.TotalDays).IsRequired();
        builder.Property(l => l.Status).IsRequired().HasConversion<int>();
        builder.Property(l => l.RequestedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(l => l.CreatedBy).HasMaxLength(100);
        builder.Property(l => l.ModifiedBy).HasMaxLength(100);

        builder.Property(l => l.Reason).HasMaxLength(500);
        builder.Property(l => l.HalfDaySession).HasMaxLength(2);

        builder.Property(l => l.IsActive).HasDefaultValue(true);

        builder.HasIndex(l => new { l.EmployeeId, l.StartDate });
    }
}
