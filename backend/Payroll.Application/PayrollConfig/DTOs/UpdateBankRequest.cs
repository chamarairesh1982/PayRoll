namespace Payroll.Application.PayrollConfig.DTOs;

public class UpdateBankRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
