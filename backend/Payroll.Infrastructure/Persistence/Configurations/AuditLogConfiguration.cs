using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Auditing;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.EntityName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(200).IsRequired();
        builder.Property(a => a.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(a => a.BeforeSnapshot).IsRequired();
        builder.Property(a => a.AfterSnapshot).IsRequired();

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.CreatedBy);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Action);
    }
}
