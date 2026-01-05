using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Leave;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class LeaveEncashmentRequestConfiguration : IEntityTypeConfiguration<LeaveEncashmentRequest>
{
    public void Configure(EntityTypeBuilder<LeaveEncashmentRequest> builder)
    {
        builder.ToTable("LeaveEncashmentRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Days).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
