using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public class BankBranchService : IBankBranchService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public BankBranchService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<BankBranchDto>> GetAsync(int page, int pageSize, Guid? bankId, string? search, bool? isActive)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.BankBranches
            .AsNoTracking()
            .Include(branch => branch.Bank)
            .AsQueryable();

        if (bankId.HasValue)
        {
            query = query.Where(branch => branch.BankId == bankId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(branch => branch.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(branch =>
                EF.Functions.Like(branch.Code, $"%{term}%") ||
                EF.Functions.Like(branch.Name, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();
        var branches = await query
            .OrderBy(branch => branch.Bank.Code)
            .ThenBy(branch => branch.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<BankBranchDto>
        {
            Items = branches.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BankBranchDto?> GetByIdAsync(Guid id)
    {
        var branch = await _dbContext.BankBranches
            .AsNoTracking()
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.Id == id);

        return branch is null ? null : MapToDto(branch);
    }

    public async Task<BankBranchDto> CreateAsync(CreateBankBranchRequest request)
    {
        var bank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Id == request.BankId);
        if (bank is null)
        {
            throw new KeyNotFoundException("Bank not found");
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        var codeExists = await _dbContext.BankBranches.AnyAsync(b => b.BankId == request.BankId && b.Code == code);
        if (codeExists)
        {
            throw new InvalidOperationException("Branch code must be unique per bank.");
        }

        var branch = new BankBranch
        {
            Id = Guid.NewGuid(),
            BankId = request.BankId,
            Code = code,
            Name = name,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.BankBranches.AddAsync(branch);
        await _dbContext.SaveChangesAsync();

        branch.Bank = bank;
        return MapToDto(branch);
    }

    public async Task UpdateAsync(Guid id, UpdateBankBranchRequest request)
    {
        var branch = await _dbContext.BankBranches.Include(b => b.Bank).FirstOrDefaultAsync(b => b.Id == id);
        if (branch is null)
        {
            throw new KeyNotFoundException("Bank branch not found");
        }

        var originalBankId = branch.BankId;
        var updatedBankId = branch.BankId;

        if (request.BankId.HasValue && request.BankId.Value != branch.BankId)
        {
            var bank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Id == request.BankId.Value);
            if (bank is null)
            {
                throw new KeyNotFoundException("Bank not found");
            }

            branch.BankId = bank.Id;
            branch.Bank = bank;
            updatedBankId = bank.Id;
        }

        var updatedCode = branch.Code;

        if (request.Code != null)
        {
            var code = request.Code.Trim();
            updatedCode = code;
        }

        if ((request.Code != null) || (request.BankId.HasValue && request.BankId.Value != originalBankId))
        {
            var codeExists = await _dbContext.BankBranches.AnyAsync(b =>
                b.BankId == updatedBankId && b.Code == updatedCode && b.Id != id);
            if (codeExists)
            {
                throw new InvalidOperationException("Branch code must be unique per bank.");
            }
        }

        if (request.Code != null)
        {
            branch.Code = updatedCode;
        }

        if (request.Name != null)
        {
            branch.Name = request.Name.Trim();
        }

        if (request.IsActive.HasValue)
        {
            branch.IsActive = request.IsActive.Value;
        }

        branch.ModifiedAt = DateTime.UtcNow;
        branch.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var branch = await _dbContext.BankBranches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch is null)
        {
            throw new KeyNotFoundException("Bank branch not found");
        }

        branch.IsActive = false;
        branch.ModifiedAt = DateTime.UtcNow;
        branch.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static BankBranchDto MapToDto(BankBranch branch)
    {
        return new BankBranchDto
        {
            Id = branch.Id,
            BankId = branch.BankId,
            BankCode = branch.Bank?.Code ?? string.Empty,
            BankName = branch.Bank?.Name ?? string.Empty,
            Code = branch.Code,
            Name = branch.Name,
            IsActive = branch.IsActive
        };
    }
}
