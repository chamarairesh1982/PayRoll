using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;
using System;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AllowanceTypeConfiguration : IEntityTypeConfiguration<AllowanceType>
{
    public void Configure(EntityTypeBuilder<AllowanceType> builder)
    {
        builder.ToTable("AllowanceTypes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Basis)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(100);

        builder.HasData(
            new AllowanceType
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "BASIC",
                Name = "Basic Salary",
                Basis = AllowanceBasis.FixedAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true,
                IsActive = true
            },
            new AllowanceType
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "TRA",
                Name = "Transport Allowance",
                Basis = AllowanceBasis.FixedAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true,
                IsActive = true
            },
            new AllowanceType
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "ATD",
                Name = "Attendance Allowance",
                Basis = AllowanceBasis.FixedAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true,
                IsActive = true
            }
        );
    }
}
