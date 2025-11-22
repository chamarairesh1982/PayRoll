using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;
using System;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class DeductionTypeConfiguration : IEntityTypeConfiguration<DeductionType>
{
    public void Configure(EntityTypeBuilder<DeductionType> builder)
    {
        builder.ToTable("DeductionTypes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(d => d.Code).IsUnique();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Basis)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedBy)
            .HasMaxLength(100);

        builder.Property(d => d.ModifiedBy)
            .HasMaxLength(100);

        builder.HasData(
            new DeductionType
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Code = "EPF_EE",
                Name = "Employee EPF",
                Basis = DeductionBasis.FixedAmount,
                IsPreTax = true,
                IsPostTax = false,
                IsActive = true
            },
            new DeductionType
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Code = "LOAN",
                Name = "Loan Installment",
                Basis = DeductionBasis.FixedAmount,
                IsPreTax = true,
                IsPostTax = false,
                IsActive = true
            },
            new DeductionType
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Code = "NOPAY",
                Name = "No Pay Deduction",
                Basis = DeductionBasis.FixedAmount,
                IsPreTax = true,
                IsPostTax = false,
                IsActive = true
            },
            new DeductionType
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Code = "PAYE",
                Name = "PAYE Tax",
                Basis = DeductionBasis.FixedAmount,
                IsPreTax = false,
                IsPostTax = true,
                IsActive = true
            }
        );
    }
}
