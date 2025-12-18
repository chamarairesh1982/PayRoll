using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Payroll;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayRunApprovalConfiguration : IEntityTypeConfiguration<PayRunApproval>
{
    public void Configure(EntityTypeBuilder<PayRunApproval> builder)
    {
        builder.ToTable("PayRunApprovals");
        builder.HasKey(pra => pra.Id);

        builder.Property(pra => pra.ActorUserId).HasMaxLength(100);
        builder.Property(pra => pra.ActorUserName).IsRequired().HasMaxLength(200);
        builder.Property(pra => pra.Comment).HasMaxLength(1000);
        builder.Property(pra => pra.FromStatus).HasConversion<int>();
        builder.Property(pra => pra.ToStatus).HasConversion<int>();
        builder.Property(pra => pra.ActionedAt).IsRequired();

        builder.HasOne(pra => pra.PayRun)
            .WithMany(pr => pr.Approvals)
            .HasForeignKey(pra => pra.PayRunId);
    }
}
