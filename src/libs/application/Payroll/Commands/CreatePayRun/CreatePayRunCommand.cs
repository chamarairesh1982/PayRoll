using MediatR;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Domain.Payroll;

namespace Payroll.Application.Payroll.Commands.CreatePayRun;

public record CreatePayRunCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public DateOnly PeriodStart { get; init; }
    public DateOnly PeriodEnd { get; init; }
    public DateOnly PaymentDate { get; init; }
}

public class CreatePayRunCommandHandler : IRequestHandler<CreatePayRunCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePayRunCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePayRunCommand request, CancellationToken cancellationToken)
    {
        // 1. Create PayRun Header
        var payRun = new PayRun
        {
            Name = request.Name,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            PaymentDate = request.PaymentDate,
            Status = PayRunStatus.Draft
        };

        // 2. Fetch Active Employees
        // Note: Global query filter automatically handles TenantId scoping
        var employees = await _context.Employees.ToListAsync(cancellationToken);

        // 3. Generate Line Items for each employee
        foreach (var employee in employees)
        {
            var lineItem = new PayRunLineItem
            {
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                BaseSalary = employee.BaseSalary,
                Allowances = 0, // Should come from employee configuration/settings
                Overtime = 0,   // Should come from input/timesheet
                OtherDeductions = 0
            };

            // Calculate Statutory Deductions (EPF, ETF, Tax)
            lineItem.Calculate();

            payRun.LineItems.Add(lineItem);
        }

        // 4. Calculate Pay Run Totals
        payRun.CalculateTotals();

        // 5. Persist
        _context.PayRuns.Add(payRun);
        await _context.SaveChangesAsync(cancellationToken);

        return payRun.Id;
    }
}
