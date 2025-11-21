using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Employees;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.EmployeeCode).IsUnique();

        builder.Property(e => e.NicNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.NicNumber).IsUnique();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Initials)
            .HasMaxLength(20);

        builder.Property(e => e.CallingName)
            .HasMaxLength(100);

        builder.Property(e => e.Gender)
            .HasConversion<int>();

        builder.Property(e => e.MaritalStatus)
            .HasConversion<int>();

        builder.Property(e => e.BaseSalary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(100);
    }
}
