using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.GeneralLedger;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class GlMappingConfiguration : IEntityTypeConfiguration<GlMapping>
{
    public void Configure(EntityTypeBuilder<GlMapping> builder)
    {
        builder.ToTable("GlMappings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.PayComponentCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => new { m.PayComponentCode, m.PayComponentType, m.CostCenterId }).IsUnique();
        builder.HasOne(m => m.DebitAccount)
            .WithMany()
            .HasForeignKey(m => m.DebitAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.CreditAccount)
            .WithMany()
            .HasForeignKey(m => m.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.CostCenter)
            .WithMany()
            .HasForeignKey(m => m.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
