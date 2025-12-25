using MediatR;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Contracts.Payroll;

namespace Payroll.Application.Payroll.Queries.GetPayRuns;

public record GetPayRunsQuery : IRequest<IEnumerable<PayRunDto>>;

public class GetPayRunsQueryHandler : IRequestHandler<GetPayRunsQuery, IEnumerable<PayRunDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPayRunsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PayRunDto>> Handle(GetPayRunsQuery request, CancellationToken cancellationToken)
    {
        return await _context.PayRuns
            .OrderByDescending(p => p.PeriodStart)
            .Select(p => new PayRunDto
            {
                Id = p.Id,
                Name = p.Name,
                PeriodStart = p.PeriodStart,
                PeriodEnd = p.PeriodEnd,
                PaymentDate = p.PaymentDate,
                Status = p.Status.ToString(),
                TotalGross = p.TotalGross,
                TotalDeductions = p.TotalDeductions,
                TotalNet = p.TotalNet,
                EmployeeCount = p.LineItems.Count
            })
            .ToListAsync(cancellationToken);
    }
}
