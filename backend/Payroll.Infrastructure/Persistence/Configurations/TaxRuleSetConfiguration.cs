using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;
using System;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class TaxRuleSetConfiguration : IEntityTypeConfiguration<TaxRuleSet>
{
    public void Configure(EntityTypeBuilder<TaxRuleSet> builder)
    {
        builder.ToTable("TaxRuleSets");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.YearOfAssessment)
            .IsRequired();

        builder.Property(r => r.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Frequency)
            .IsRequired()
            .HasDefaultValue(TaxRuleSetFrequency.Monthly);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(r => new { r.YearOfAssessment, r.IsActive, r.IsDefault });

        builder.HasData(new TaxRuleSet
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Name = "Sri Lanka PAYE YA 2025/26",
            YearOfAssessment = 2025,
            EffectiveFrom = new DateOnly(2025, 4, 1),
            Frequency = TaxRuleSetFrequency.Monthly,
            CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "system",
            IsDefault = true,
            IsActive = true
        });
    }
}
