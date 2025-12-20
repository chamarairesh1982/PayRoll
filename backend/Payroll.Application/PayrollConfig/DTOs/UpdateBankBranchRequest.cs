namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateBankBranchRequest
{
    public Guid? BankId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
