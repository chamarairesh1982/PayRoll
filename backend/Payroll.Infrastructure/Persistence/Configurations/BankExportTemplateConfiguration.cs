using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class BankExportTemplateConfiguration : IEntityTypeConfiguration<BankExportTemplate>
{
    public void Configure(EntityTypeBuilder<BankExportTemplate> builder)
    {
        builder.ToTable("BankExportTemplates");
        builder.HasKey(template => template.Id);
        builder.Property(template => template.Name).HasMaxLength(200).IsRequired();
        builder.Property(template => template.Format).HasConversion<int>();
        builder.Property(template => template.Delimiter).HasMaxLength(10);
        builder.Property(template => template.HeaderRowCount).HasDefaultValue(0);
        builder.Property(template => template.IsActive).HasDefaultValue(true);
        builder.Property(template => template.CreatedBy).HasMaxLength(100);
        builder.Property(template => template.ModifiedBy).HasMaxLength(100);
    }
}
