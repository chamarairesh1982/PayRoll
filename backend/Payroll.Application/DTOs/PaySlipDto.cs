using System.Collections.Generic;
using System.Linq;

namespace Payroll.Application.DTOs;

public class PaySlipDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public decimal EmployeeEpf { get; set; }
    public decimal EmployerEpf { get; set; }
    public decimal EmployerEtf { get; set; }
    public decimal PayeTax { get; set; }
    public IEnumerable<EarningDto> Earnings { get; set; } = Enumerable.Empty<EarningDto>();
    public IEnumerable<DeductionDto> Deductions { get; set; } = Enumerable.Empty<DeductionDto>();
}

public record EarningDto(string Code, string Description, decimal Amount, bool IsEpfApplicable, bool IsEtfApplicable, bool IsTaxable);

public record DeductionDto(string Code, string Description, decimal Amount, bool IsPreTax, bool IsPostTax);
