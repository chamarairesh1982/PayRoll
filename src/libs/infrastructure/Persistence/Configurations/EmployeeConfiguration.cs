using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(t => t.EmployeeCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.BaseSalary)
            .HasPrecision(18, 2);

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Unique constraint on EmployeeCode per Tenant
        builder.HasIndex(t => new { t.TenantId, t.EmployeeCode })
            .IsUnique();
    }
}
