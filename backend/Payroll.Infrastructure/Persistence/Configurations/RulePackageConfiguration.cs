using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class RulePackageConfiguration : IEntityTypeConfiguration<RulePackage>
{
    public void Configure(EntityTypeBuilder<RulePackage> builder)
    {
        builder.ToTable("RulePackages");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.CompanyId).IsRequired();
        builder.Property(rp => rp.RuleType).HasConversion<int>();
        builder.Property(rp => rp.Name).IsRequired().HasMaxLength(200);
        builder.Property(rp => rp.CreatedBy).HasMaxLength(100);
        builder.Property(rp => rp.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(rp => new { rp.CompanyId, rp.RuleType, rp.Name }).IsUnique();

        builder.HasMany(rp => rp.Versions)
            .WithOne(v => v.RulePackage)
            .HasForeignKey(v => v.RulePackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
