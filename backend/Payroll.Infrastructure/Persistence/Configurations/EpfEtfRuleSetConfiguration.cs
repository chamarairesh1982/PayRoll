using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class EpfEtfRuleSetConfiguration : IEntityTypeConfiguration<EpfEtfRuleSet>
{
    public void Configure(EntityTypeBuilder<EpfEtfRuleSet> builder)
    {
        builder.ToTable("EpfEtfRuleSets");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.EmployeeEpfRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(r => r.EmployerEpfRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(r => r.EmployerEtfRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(r => r.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(r => new { r.EffectiveFrom, r.IsActive, r.IsDefault });
    }
}
