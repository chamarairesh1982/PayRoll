namespace Payroll.Contracts.Payroll;

public class PayRunDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public int EmployeeCount { get; set; }
}

public class PayRunDetailDto : PayRunDto
{
    public IEnumerable<PayRunLineItemDto> LineItems { get; set; } = Enumerable.Empty<PayRunLineItemDto>();
}

public class PayRunLineItemDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Overtime { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal EpfEmployee { get; set; }
    public decimal Tax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }
}
