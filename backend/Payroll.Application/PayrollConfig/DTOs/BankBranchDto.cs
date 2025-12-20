namespace Payroll.Application.PayrollConfig.DTOs;

public class BankBranchDto
{
    public Guid Id { get; set; }
    public Guid BankId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
