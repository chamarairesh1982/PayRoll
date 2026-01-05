using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.GeneralLedger;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class GeneralLedgerAccountMappingConfiguration : IEntityTypeConfiguration<GeneralLedgerAccountMapping>
{
    public void Configure(EntityTypeBuilder<GeneralLedgerAccountMapping> builder)
    {
        builder.ToTable("GeneralLedgerAccountMappings");

        builder.Property(m => m.Code)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(m => m.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(m => m.DebitAccount)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(m => m.CreditAccount)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(m => m.Notes)
            .HasMaxLength(1024);

        builder.HasIndex(m => new { m.Code, m.MappingType })
            .IsUnique();
    }
}
