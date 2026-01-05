using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public class AllowanceTypeService : IAllowanceTypeService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AllowanceTypeService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<AllowanceTypeDto>> GetAsync(int page, int pageSize, bool? isActive)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.AllowanceTypes.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var allowances = await query
            .OrderBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = allowances.Select(MapToDto).ToList();

        return new PaginatedResult<AllowanceTypeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AllowanceTypeDto?> GetByIdAsync(Guid id)
    {
        var allowance = await _dbContext.AllowanceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return allowance is null ? null : MapToDto(allowance);
    }

    public async Task<AllowanceTypeDto> CreateAsync(CreateAllowanceTypeRequest request)
    {
        var normalizedCode = request.Code.Trim();
        var normalizedName = request.Name.Trim();

        var codeExists = await _dbContext.AllowanceTypes.AnyAsync(a => a.Code == normalizedCode);
        if (codeExists)
        {
            throw new InvalidOperationException("Allowance code must be unique.");
        }

        var allowance = new AllowanceType
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            Basis = request.Basis,
            IsEpfApplicable = request.IsEpfApplicable,
            IsEtfApplicable = request.IsEtfApplicable,
            IsTaxable = request.IsTaxable,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.AllowanceTypes.AddAsync(allowance);
        await _dbContext.SaveChangesAsync();

        return MapToDto(allowance);
    }

    public async Task UpdateAsync(Guid id, UpdateAllowanceTypeRequest request)
    {
        var allowance = await _dbContext.AllowanceTypes.FirstOrDefaultAsync(a => a.Id == id);
        if (allowance is null)
        {
            throw new KeyNotFoundException("Allowance type not found");
        }

        if (request.Name != null)
        {
            allowance.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            allowance.Description = request.Description.Trim();
        }

        if (request.Basis.HasValue)
        {
            allowance.Basis = request.Basis.Value;
        }

        if (request.IsEpfApplicable.HasValue)
        {
            allowance.IsEpfApplicable = request.IsEpfApplicable.Value;
        }

        if (request.IsEtfApplicable.HasValue)
        {
            allowance.IsEtfApplicable = request.IsEtfApplicable.Value;
        }

        if (request.IsTaxable.HasValue)
        {
            allowance.IsTaxable = request.IsTaxable.Value;
        }

        if (request.IsActive.HasValue)
        {
            allowance.IsActive = request.IsActive.Value;
        }

        allowance.ModifiedAt = DateTime.UtcNow;
        allowance.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var allowance = await _dbContext.AllowanceTypes.FirstOrDefaultAsync(a => a.Id == id);
        if (allowance is null)
        {
            throw new KeyNotFoundException("Allowance type not found");
        }

        allowance.IsActive = false;
        allowance.ModifiedAt = DateTime.UtcNow;
        allowance.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static AllowanceTypeDto MapToDto(AllowanceType allowance)
    {
        return new AllowanceTypeDto
        {
            Id = allowance.Id,
            Code = allowance.Code,
            Name = allowance.Name,
            Description = allowance.Description,
            Basis = allowance.Basis,
            IsEpfApplicable = allowance.IsEpfApplicable,
            IsEtfApplicable = allowance.IsEtfApplicable,
            IsTaxable = allowance.IsTaxable,
            IsActive = allowance.IsActive
        };
    }
}
