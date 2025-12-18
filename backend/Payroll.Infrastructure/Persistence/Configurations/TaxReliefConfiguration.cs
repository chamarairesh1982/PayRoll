using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class TaxReliefConfiguration : IEntityTypeConfiguration<TaxRelief>
{
    public void Configure(EntityTypeBuilder<TaxRelief> builder)
    {
        builder.ToTable("TaxReliefs");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.ReliefType)
            .IsRequired();

        builder.Property(r => r.Frequency)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100);

        builder.HasOne(r => r.TaxRuleSet)
            .WithMany(rs => rs.Reliefs)
            .HasForeignKey(r => r.TaxRuleSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
