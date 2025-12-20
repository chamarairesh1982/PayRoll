namespace Payroll.Application.DTOs;

public class PayslipBulkGenerateResultDto
{
    public int GeneratedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
}
