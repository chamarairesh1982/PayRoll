using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public class BankService : IBankService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public BankService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<BankDto>> GetAsync(int page, int pageSize, string? search, bool? isActive)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.Banks.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(bank => bank.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(bank =>
                EF.Functions.Like(bank.Code, $"%{term}%") ||
                EF.Functions.Like(bank.Name, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();
        var banks = await query
            .OrderBy(bank => bank.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<BankDto>
        {
            Items = banks.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BankDto?> GetByIdAsync(Guid id)
    {
        var bank = await _dbContext.Banks.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        return bank is null ? null : MapToDto(bank);
    }

    public async Task<BankDto> CreateAsync(CreateBankRequest request)
    {
        var code = request.Code.Trim();
        var name = request.Name.Trim();

        var codeExists = await _dbContext.Banks.AnyAsync(b => b.Code == code);
        if (codeExists)
        {
            throw new InvalidOperationException("Bank code must be unique.");
        }

        var bank = new Bank
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.Banks.AddAsync(bank);
        await _dbContext.SaveChangesAsync();

        return MapToDto(bank);
    }

    public async Task UpdateAsync(Guid id, UpdateBankRequest request)
    {
        var bank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Id == id);
        if (bank is null)
        {
            throw new KeyNotFoundException("Bank not found");
        }

        if (request.Code != null)
        {
            var code = request.Code.Trim();
            var codeExists = await _dbContext.Banks.AnyAsync(b => b.Code == code && b.Id != id);
            if (codeExists)
            {
                throw new InvalidOperationException("Bank code must be unique.");
            }

            bank.Code = code;
        }

        if (request.Name != null)
        {
            bank.Name = request.Name.Trim();
        }

        if (request.IsActive.HasValue)
        {
            bank.IsActive = request.IsActive.Value;
        }

        bank.ModifiedAt = DateTime.UtcNow;
        bank.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var bank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Id == id);
        if (bank is null)
        {
            throw new KeyNotFoundException("Bank not found");
        }

        bank.IsActive = false;
        bank.ModifiedAt = DateTime.UtcNow;
        bank.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static BankDto MapToDto(Bank bank)
    {
        return new BankDto
        {
            Id = bank.Id,
            Code = bank.Code,
            Name = bank.Name,
            IsActive = bank.IsActive
        };
    }
}
