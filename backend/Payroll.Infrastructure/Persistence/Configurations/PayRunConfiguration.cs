using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.Organizations;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunConfiguration : IEntityTypeConfiguration<PayRun>
{
    public void Configure(EntityTypeBuilder<PayRun> builder)
    {
        builder.ToTable("PayRuns");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Reference).IsRequired().HasMaxLength(50);
        builder.Property(pr => pr.Code).IsRequired().HasMaxLength(50);
        builder.Property(pr => pr.Name).IsRequired().HasMaxLength(200);
        builder.Property(pr => pr.PayDate).IsRequired();
        builder.Property(pr => pr.PeriodStart).IsRequired();
        builder.Property(pr => pr.PeriodEnd).IsRequired();
        builder.Property(pr => pr.Status).HasConversion<int>();
        builder.Property(pr => pr.PeriodType).HasConversion<int>();
        builder.Property(pr => pr.IsLocked).HasDefaultValue(false);
        builder.Property(pr => pr.IsConsolidated).HasDefaultValue(false);
        builder.Property(pr => pr.PreparedByUserId).HasMaxLength(100);
        builder.Property(pr => pr.PreparedByUserName).HasMaxLength(200);
        builder.Property(pr => pr.ApprovedByUserId).HasMaxLength(100);
        builder.Property(pr => pr.ApprovedByUserName).HasMaxLength(200);
        builder.Property(pr => pr.LockedByUserId).HasMaxLength(100);
        builder.Property(pr => pr.LockedByUserName).HasMaxLength(200);
        builder.Property(pr => pr.RulesSnapshotJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(pr => pr.Reference).IsUnique();
        builder.HasIndex(pr => pr.Code).IsUnique();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(pr => pr.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(pr => pr.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(pr => pr.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Company)
               .WithMany()
               .HasForeignKey(p => p.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Branch)
               .WithMany()
               .HasForeignKey(p => p.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CostCenter)
               .WithMany()
               .HasForeignKey(p => p.CostCenterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RulePackageVersion>()
            .WithMany()
            .HasForeignKey(pr => pr.TaxRuleVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RulePackageVersion>()
            .WithMany()
            .HasForeignKey(pr => pr.EpfRuleVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RulePackageVersion>()
            .WithMany()
            .HasForeignKey(pr => pr.EtfRuleVersionId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
