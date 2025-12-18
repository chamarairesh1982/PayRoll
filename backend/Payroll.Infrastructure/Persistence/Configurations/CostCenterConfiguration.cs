using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Organizations;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(cc => cc.Code).IsUnique();

        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(cc => cc.Branch)
            .WithMany(b => b.CostCenters)
            .HasForeignKey(cc => cc.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
