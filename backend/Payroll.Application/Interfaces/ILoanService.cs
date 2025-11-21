using Payroll.Application.DTOs;
using Payroll.Shared;

namespace Payroll.Application.Interfaces;

public interface ILoanService
{
    Task<PaginatedResult<LoanDto>> GetLoansAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<LoanDto?> GetLoanAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoanDto> CreateLoanAsync(LoanDto loan, CancellationToken cancellationToken = default);
    Task RecordRepaymentAsync(Guid id, decimal amount, CancellationToken cancellationToken = default);
}

public record LoanDto(Guid Id, Guid EmployeeId, decimal Principal, decimal Outstanding, string Status);
