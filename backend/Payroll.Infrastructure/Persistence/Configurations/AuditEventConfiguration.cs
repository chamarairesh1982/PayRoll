using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Auditing;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");

        builder.Property(a => a.ActorUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.ActorDisplayName)
            .HasMaxLength(200);

        builder.Property(a => a.EntityType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.Property(a => a.PreviousHash)
            .HasMaxLength(128);

        builder.Property(a => a.Hash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(a => a.TimestampUtc);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
