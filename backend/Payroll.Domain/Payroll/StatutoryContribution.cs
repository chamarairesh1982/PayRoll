namespace Payroll.Domain.Payroll;

public class StatutoryContribution
{
    public string Name { get; set; } = string.Empty;
    public decimal EmployerPortion { get; set; }
    public decimal EmployeePortion { get; set; }
}
