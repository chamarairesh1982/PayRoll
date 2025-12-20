using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.GeneralLedger;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class GlJournalBatchConfiguration : IEntityTypeConfiguration<GlJournalBatch>
{
    public void Configure(EntityTypeBuilder<GlJournalBatch> builder)
    {
        builder.ToTable("GlJournalBatches");
        builder.HasKey(b => b.Id);
        builder.HasOne(b => b.PayRun)
            .WithMany()
            .HasForeignKey(b => b.PayRunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(b => b.Lines)
            .WithOne(l => l.Batch)
            .HasForeignKey(l => l.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.Notes).HasMaxLength(500);
    }
}
