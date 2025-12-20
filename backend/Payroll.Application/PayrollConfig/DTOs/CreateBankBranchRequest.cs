namespace Payroll.Application.PayrollConfig.DTOs;

public class CreateBankBranchRequest
{
    public Guid BankId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
