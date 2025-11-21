namespace Payroll.Application.Payroll.Requests;

public class RecalculatePayRunRequest
{
    public bool IncludeAttendance { get; set; } = true;
    public bool IncludeOvertime { get; set; } = true;
    public bool IncludeLoans { get; set; } = true;
    public bool IncludeAllowancesAndDeductions { get; set; } = true;
}
