using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunStatusHistoryConfiguration : IEntityTypeConfiguration<PayRunStatusHistory>
{
    public void Configure(EntityTypeBuilder<PayRunStatusHistory> builder)
    {
        builder.ToTable("PayRunStatusHistory");

        builder.Property(p => p.ActorUserId)
            .HasMaxLength(100);

        builder.Property(p => p.ActorDisplayName)
            .HasMaxLength(200);

        builder.Property(p => p.Comment)
            .HasMaxLength(1000);

        builder.Property(p => p.PreviousHash)
            .HasMaxLength(128);

        builder.Property(p => p.Hash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(p => p.PayRunId);
    }
}
