using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class AllowanceTypeConfiguration : IEntityTypeConfiguration<AllowanceType>
{
    public void Configure(EntityTypeBuilder<AllowanceType> builder)
    {
        builder.ToTable("AllowanceTypes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Basis)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(100);
    }
}
