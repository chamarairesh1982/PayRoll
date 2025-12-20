using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.GeneralLedger;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        builder.ToTable("GlAccounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();
    }
}
