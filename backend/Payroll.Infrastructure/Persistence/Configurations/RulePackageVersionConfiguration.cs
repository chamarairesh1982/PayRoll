using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class RulePackageVersionConfiguration : IEntityTypeConfiguration<RulePackageVersion>
{
    public void Configure(EntityTypeBuilder<RulePackageVersion> builder)
    {
        builder.ToTable("RulePackageVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionNumber).IsRequired();
        builder.Property(v => v.EffectiveFrom).IsRequired().HasColumnType("date");
        builder.Property(v => v.EffectiveTo).HasColumnType("date");
        builder.Property(v => v.Status).HasConversion<int>();
        builder.Property(v => v.ContentJson).IsRequired();
        builder.Property(v => v.ContentHash).IsRequired().HasMaxLength(128);
        builder.Property(v => v.CreatedBy).HasMaxLength(100);
        builder.Property(v => v.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(v => new { v.RulePackageId, v.VersionNumber }).IsUnique();
        builder.HasIndex(v => new { v.RulePackageId, v.EffectiveFrom });

        builder.HasCheckConstraint(
            "CK_RulePackageVersions_EffectiveDates",
            "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");
    }
}
