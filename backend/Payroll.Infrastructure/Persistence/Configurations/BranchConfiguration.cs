using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Organizations;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.Code).IsUnique();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
