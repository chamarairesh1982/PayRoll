using Payroll.Domain.Payroll;

namespace Payroll.Application.Payroll.Requests;

public class ChangePayRunStatusRequest
{
    public PayRunStatus Status { get; set; }
}
