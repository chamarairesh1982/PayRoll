using Payroll.Domain.Payroll;

namespace Payroll.Application.DTOs;

public class PayRunDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public PayRunStatus Status { get; set; }
}
