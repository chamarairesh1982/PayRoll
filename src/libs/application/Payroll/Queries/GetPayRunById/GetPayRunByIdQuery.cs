using MediatR;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Contracts.Payroll;

namespace Payroll.Application.Payroll.Queries.GetPayRunById;

public record GetPayRunByIdQuery(Guid Id) : IRequest<PayRunDetailDto?>;

public class GetPayRunByIdQueryHandler : IRequestHandler<GetPayRunByIdQuery, PayRunDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPayRunByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayRunDetailDto?> Handle(GetPayRunByIdQuery request, CancellationToken cancellationToken)
    {
        var payRun = await _context.PayRuns
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (payRun == null) return null;

        return new PayRunDetailDto
        {
            Id = payRun.Id,
            Name = payRun.Name,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PaymentDate = payRun.PaymentDate,
            Status = payRun.Status.ToString(),
            TotalGross = payRun.TotalGross,
            TotalDeductions = payRun.TotalDeductions,
            TotalNet = payRun.TotalNet,
            EmployeeCount = payRun.LineItems.Count,
            LineItems = payRun.LineItems.Select(li => new PayRunLineItemDto
            {
                Id = li.Id,
                EmployeeId = li.EmployeeId,
                EmployeeCode = li.EmployeeCode,
                EmployeeName = li.EmployeeName,
                BaseSalary = li.BaseSalary,
                Allowances = li.Allowances,
                Overtime = li.Overtime,
                GrossAmount = li.GrossAmount,
                EpfEmployee = li.EpfEmployee,
                Tax = li.Tax,
                OtherDeductions = li.OtherDeductions,
                TotalDeductions = li.TotalDeductions,
                NetAmount = li.NetAmount
            }).ToList()
        };
    }
}
