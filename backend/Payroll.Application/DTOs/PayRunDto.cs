using Payroll.Domain.Payroll;
using System.Collections.Generic;
using System.Linq;

namespace Payroll.Application.DTOs;

public class PayRunDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public bool IsLocked { get; set; }
    public PayRunStatus Status { get; set; }
    public List<Guid>? EmployeeIds { get; set; }
    public IEnumerable<PaySlipDto> PaySlips { get; set; } = Enumerable.Empty<PaySlipDto>();
}
