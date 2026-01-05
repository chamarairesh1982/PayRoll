using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public class DeductionTypeService : IDeductionTypeService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeductionTypeService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<DeductionTypeDto>> GetAsync(int page, int pageSize, bool? isActive)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.DeductionTypes.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var deductions = await query
            .OrderBy(d => d.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = deductions.Select(MapToDto).ToList();

        return new PaginatedResult<DeductionTypeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DeductionTypeDto?> GetByIdAsync(Guid id)
    {
        var deduction = await _dbContext.DeductionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return deduction is null ? null : MapToDto(deduction);
    }

    public async Task<DeductionTypeDto> CreateAsync(CreateDeductionTypeRequest request)
    {
        var normalizedCode = request.Code.Trim();
        var normalizedName = request.Name.Trim();

        var codeExists = await _dbContext.DeductionTypes.AnyAsync(d => d.Code == normalizedCode);
        if (codeExists)
        {
            throw new InvalidOperationException("Deduction code must be unique.");
        }

        var deduction = new DeductionType
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            Basis = request.Basis,
            IsPreTax = request.IsPreTax,
            IsPostTax = request.IsPostTax,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.DeductionTypes.AddAsync(deduction);
        await _dbContext.SaveChangesAsync();

        return MapToDto(deduction);
    }

    public async Task UpdateAsync(Guid id, UpdateDeductionTypeRequest request)
    {
        var deduction = await _dbContext.DeductionTypes.FirstOrDefaultAsync(d => d.Id == id);
        if (deduction is null)
        {
            throw new KeyNotFoundException("Deduction type not found");
        }

        if (request.Name != null)
        {
            deduction.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            deduction.Description = request.Description.Trim();
        }

        if (request.Basis.HasValue)
        {
            deduction.Basis = request.Basis.Value;
        }

        if (request.IsPreTax.HasValue)
        {
            deduction.IsPreTax = request.IsPreTax.Value;
        }

        if (request.IsPostTax.HasValue)
        {
            deduction.IsPostTax = request.IsPostTax.Value;
        }

        if (request.IsActive.HasValue)
        {
            deduction.IsActive = request.IsActive.Value;
        }

        deduction.ModifiedAt = DateTime.UtcNow;
        deduction.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var deduction = await _dbContext.DeductionTypes.FirstOrDefaultAsync(d => d.Id == id);
        if (deduction is null)
        {
            throw new KeyNotFoundException("Deduction type not found");
        }

        deduction.IsActive = false;
        deduction.ModifiedAt = DateTime.UtcNow;
        deduction.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private static DeductionTypeDto MapToDto(DeductionType deduction)
    {
        return new DeductionTypeDto
        {
            Id = deduction.Id,
            Code = deduction.Code,
            Name = deduction.Name,
            Description = deduction.Description,
            Basis = deduction.Basis,
            IsPreTax = deduction.IsPreTax,
            IsPostTax = deduction.IsPostTax,
            IsActive = deduction.IsActive
        };
    }
}
