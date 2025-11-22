using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunConfiguration : IEntityTypeConfiguration<PayRun>
{
    public void Configure(EntityTypeBuilder<PayRun> builder)
    {
        builder.ToTable("PayRuns");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Reference).IsRequired().HasMaxLength(50);
        builder.Property(pr => pr.Code).IsRequired().HasMaxLength(50);
        builder.Property(pr => pr.Name).IsRequired().HasMaxLength(200);
        builder.Property(pr => pr.PayDate).IsRequired();
        builder.Property(pr => pr.PeriodStart).IsRequired();
        builder.Property(pr => pr.PeriodEnd).IsRequired();
        builder.Property(pr => pr.Status).HasConversion<int>();
        builder.Property(pr => pr.PeriodType).HasConversion<int>();
        builder.Property(pr => pr.IsLocked).HasDefaultValue(false);

        builder.HasIndex(pr => pr.Reference).IsUnique();
        builder.HasIndex(pr => pr.Code).IsUnique();
    }
}
