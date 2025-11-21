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

        builder.Property(pr => pr.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(pr => pr.Code)
            .IsUnique();

        builder.Property(pr => pr.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(pr => pr.PeriodType)
            .IsRequired();

        builder.Property(pr => pr.PeriodStart)
            .HasConversion(date => date.ToDateTime(TimeOnly.MinValue), dateTime => DateOnly.FromDateTime(dateTime))
            .HasColumnType("date")
            .IsRequired();

        builder.Property(pr => pr.PeriodEnd)
            .HasConversion(date => date.ToDateTime(TimeOnly.MinValue), dateTime => DateOnly.FromDateTime(dateTime))
            .HasColumnType("date")
            .IsRequired();

        builder.Property(pr => pr.PayDate)
            .HasConversion(date => date.ToDateTime(TimeOnly.MinValue), dateTime => DateOnly.FromDateTime(dateTime))
            .HasColumnType("date")
            .IsRequired();

        builder.Property(pr => pr.Status)
            .IsRequired();

        builder.Property(pr => pr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pr => pr.CreatedBy).HasMaxLength(100);
        builder.Property(pr => pr.ModifiedBy).HasMaxLength(100);
    }
}
