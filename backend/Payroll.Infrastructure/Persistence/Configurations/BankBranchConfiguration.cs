using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class BankBranchConfiguration : IEntityTypeConfiguration<BankBranch>
{
    public void Configure(EntityTypeBuilder<BankBranch> builder)
    {
        builder.ToTable("BankBranches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BankId)
            .IsRequired();

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.ModifiedBy)
            .HasMaxLength(100);

        builder.HasOne(b => b.Bank)
            .WithMany(bank => bank.Branches)
            .HasForeignKey(b => b.BankId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.BankId, b.Code })
            .IsUnique();
    }
}
