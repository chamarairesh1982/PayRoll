using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class DeductionTypeConfiguration : IEntityTypeConfiguration<DeductionType>
{
    public void Configure(EntityTypeBuilder<DeductionType> builder)
    {
        builder.ToTable("DeductionTypes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(d => d.Code).IsUnique();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Basis)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedBy)
            .HasMaxLength(100);

        builder.Property(d => d.ModifiedBy)
            .HasMaxLength(100);
    }
}
