using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

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

        builder.Property(s => s.RatePercent)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(s => s.Order)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(100);
    }
}
