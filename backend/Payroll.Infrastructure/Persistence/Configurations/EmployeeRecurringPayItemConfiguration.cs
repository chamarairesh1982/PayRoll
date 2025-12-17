using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EmployeeRecurringPayItemConfiguration : IEntityTypeConfiguration<EmployeeRecurringPayItem>
{
    public void Configure(EntityTypeBuilder<EmployeeRecurringPayItem> builder)
    {
        builder.ToTable("EmployeeRecurringPayItems");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.EmployeeId).IsRequired();

        builder.Property(pi => pi.PayItemKind)
            .IsRequired()
            .HasConversion<int>();

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

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(pi => pi.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.AllowanceType)
            .WithMany()
            .HasForeignKey(pi => pi.AllowanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.DeductionType)
            .WithMany()
            .HasForeignKey(pi => pi.DeductionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => pi.AllowanceTypeId);
        builder.HasIndex(pi => pi.DeductionTypeId);
        builder.HasIndex(pi => pi.EmployeeId);

        builder.HasIndex(pi => new
        {
            pi.EmployeeId,
            pi.PayItemKind,
            pi.AllowanceTypeId,
            pi.DeductionTypeId,
            pi.EffectiveFrom
        });

        builder.HasCheckConstraint(
            "CK_EmployeeRecurringPayItems_AmountOrPercentage",
            "((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))");

        builder.HasCheckConstraint(
            "CK_EmployeeRecurringPayItems_PositiveValues",
            "((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0))");

        builder.HasCheckConstraint(
            "CK_EmployeeRecurringPayItems_EffectiveDates",
            "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");

        builder.HasCheckConstraint(
            "CK_EmployeeRecurringPayItems_KindAndType",
            "((PayItemKind = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR " +
            "(PayItemKind = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))");
    }
}
