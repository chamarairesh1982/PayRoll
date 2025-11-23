using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Loans;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class LoanService : ILoanService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public LoanService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<LoanDto>> GetLoansAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Loans.AsNoTracking().OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => MapToDto(l))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<LoanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<LoanDto?> GetLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Loans.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<LoanDto> CreateLoanAsync(LoanDto loan, CancellationToken cancellationToken = default)
    {
        var entity = new Loan
        {
            EmployeeId = loan.EmployeeId,
            PrincipalAmount = loan.Principal,
            OutstandingPrincipal = loan.Outstanding > 0 ? loan.Outstanding : loan.Principal,
            InstallmentAmount = 0m,
            StartDate = DateTime.UtcNow,
            Status = LoanStatus.Active,
            CreatedBy = _currentUserService.UserId ?? "system"
        };

        _dbContext.Loans.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task RecordRepaymentAsync(Guid id, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var loan = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (loan is null)
        {
            throw new KeyNotFoundException("Loan not found");
        }

        loan.Repayments.Add(new LoanRepayment
        {
            DueDate = DateTime.UtcNow,
            Amount = amount,
            IsPaid = true
        });

        loan.OutstandingPrincipal -= amount;
        if (loan.OutstandingPrincipal < 0)
        {
            loan.OutstandingPrincipal = 0;
        }

        if (loan.OutstandingPrincipal == 0)
        {
            loan.Status = LoanStatus.Closed;
            loan.EndDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static LoanDto MapToDto(Loan entity) => new LoanDto(
        entity.Id,
        entity.EmployeeId,
        entity.PrincipalAmount,
        entity.OutstandingPrincipal,
        entity.Status.ToString());
}
