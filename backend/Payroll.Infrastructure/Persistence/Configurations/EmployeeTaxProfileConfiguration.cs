using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EmployeeTaxProfileConfiguration : IEntityTypeConfiguration<EmployeeTaxProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeTaxProfile> builder)
    {
        builder.ToTable("EmployeeTaxProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.IsTaxExempt)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(p => p.EmployeeId)
            .IsUnique();

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.SlabSetOverride)
            .WithMany()
            .HasForeignKey(p => p.SlabSetOverrideId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
