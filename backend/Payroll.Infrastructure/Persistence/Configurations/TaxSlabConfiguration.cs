using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;
using System;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class TaxSlabConfiguration : IEntityTypeConfiguration<TaxSlab>
{
    public void Configure(EntityTypeBuilder<TaxSlab> builder)
    {
        builder.ToTable("TaxSlabs");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.TaxRuleSet)
            .WithMany(r => r.Slabs)
            .HasForeignKey(s => s.TaxRuleSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.FromAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.ToAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Rate)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(s => s.Order)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(100);

        var ruleSetId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        builder.HasData(
            new TaxSlab
            {
                Id = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                TaxRuleSetId = ruleSetId,
                FromAmount = 0,
                ToAmount = 100000,
                Rate = 0m,
                CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system",
                Order = 1
            },
            new TaxSlab
            {
                Id = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                TaxRuleSetId = ruleSetId,
                FromAmount = 100000,
                ToAmount = 141667,
                Rate = 0.06m,
                CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system",
                Order = 2
            },
            new TaxSlab
            {
                Id = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                TaxRuleSetId = ruleSetId,
                FromAmount = 141667,
                ToAmount = 183333,
                Rate = 0.12m,
                CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system",
                Order = 3
            },
            new TaxSlab
            {
                Id = Guid.Parse("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                TaxRuleSetId = ruleSetId,
                FromAmount = 183333,
                ToAmount = null,
                Rate = 0.18m,
                CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system",
                Order = 4
            }
        );
    }
}
