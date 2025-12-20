using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Leave;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class LeaveTypeDefinitionConfiguration : IEntityTypeConfiguration<LeaveTypeDefinition>
{
    public void Configure(EntityTypeBuilder<LeaveTypeDefinition> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EncashmentRateMultiplier).HasColumnType("decimal(5,2)");
    }
}
