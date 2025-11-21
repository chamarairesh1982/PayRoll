using Payroll.Application.Interfaces;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class LoanService : ILoanService
{
    public Task<LoanDto> CreateLoanAsync(LoanDto loan, CancellationToken cancellationToken = default)
    {
        // TODO: persist new loan
        return Task.FromResult(loan);
    }

    public Task<LoanDto?> GetLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: fetch loan
        return Task.FromResult<LoanDto?>(null);
    }

    public Task<PaginatedResult<LoanDto>> GetLoansAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = new PaginatedResult<LoanDto>
        {
            Items = Array.Empty<LoanDto>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = 0
        };
        return Task.FromResult(result);
    }

    public Task RecordRepaymentAsync(Guid id, decimal amount, CancellationToken cancellationToken = default)
    {
        // TODO: record repayment
        return Task.CompletedTask;
    }
}
