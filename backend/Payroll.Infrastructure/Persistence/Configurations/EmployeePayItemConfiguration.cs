using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EmployeePayItemConfiguration : IEntityTypeConfiguration<EmployeePayItem>
{
    public void Configure(EntityTypeBuilder<EmployeePayItem> builder)
    {
        builder.ToTable("EmployeePayItems");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.EmployeeId).IsRequired();

        builder.Property(pi => pi.PayItemType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pi => pi.PayItemCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pi => pi.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(pi => pi.Percentage)
            .HasColumnType("decimal(18,2)");

        builder.Property(pi => pi.EffectiveFrom)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(pi => pi.EffectiveTo)
            .HasColumnType("date");

        builder.Property(pi => pi.IsActive)
            .HasDefaultValue(true);

        builder.Property(pi => pi.CreatedBy).HasMaxLength(100);
        builder.Property(pi => pi.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(pi => new { pi.EmployeeId, pi.PayItemCode, pi.PayItemType, pi.EffectiveFrom });

        builder.HasCheckConstraint(
            "CK_EmployeePayItems_AmountOrPercentage",
            "((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))");

        builder.HasCheckConstraint(
            "CK_EmployeePayItems_PositiveValues",
            "((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0))");

        builder.HasCheckConstraint(
            "CK_EmployeePayItems_EffectiveDates",
            "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");
    }
}
