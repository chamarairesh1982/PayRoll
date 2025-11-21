namespace Payroll.Application.DTOs;

public class PaySlipDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal NetPay { get; set; }
    public IEnumerable<EarningDto> Earnings { get; set; } = Enumerable.Empty<EarningDto>();
    public IEnumerable<DeductionDto> Deductions { get; set; } = Enumerable.Empty<DeductionDto>();
}

public record EarningDto(string Description, decimal Amount);
public record DeductionDto(string Description, decimal Amount);
