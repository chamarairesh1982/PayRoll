using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.PayrollConfig;

public class EpfEtfRuleSetService : IEpfEtfRuleSetService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public EpfEtfRuleSetService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<EpfEtfRuleSetDto>> GetAsync(int page, int pageSize, bool? isActive)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.EpfEtfRuleSets.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var ruleSets = await query
            .OrderByDescending(r => r.EffectiveFrom)
            .ThenBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = ruleSets.Select(MapToDto).ToList();

        return new PaginatedResult<EpfEtfRuleSetDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EpfEtfRuleSetDto?> GetByIdAsync(Guid id)
    {
        var ruleSet = await _dbContext.EpfEtfRuleSets
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return ruleSet is null ? null : MapToDto(ruleSet);
    }

    public async Task<EpfEtfRuleSetDto> CreateAsync(CreateEpfEtfRuleSetRequest request)
    {
        ValidateDates(request.EffectiveFrom, request.EffectiveTo);
        ValidateRates(request.EmployeeEpfRate, request.EmployerEpfRate, request.EmployerEtfRate);
        ValidateLimits(request.MinimumWageForEpf, request.MaximumEarningForEpf, request.MaximumEarningForEtf);

        if (request.IsDefault)
        {
            await ClearDefaultEpfEtfRuleSetsAsync();
        }

        var ruleSet = new EpfEtfRuleSet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            EffectiveFrom = DateOnly.FromDateTime(request.EffectiveFrom),
            EffectiveTo = request.EffectiveTo.HasValue ? DateOnly.FromDateTime(request.EffectiveTo.Value) : null,
            EmployeeEpfRate = request.EmployeeEpfRate,
            EmployerEpfRate = request.EmployerEpfRate,
            EmployerEtfRate = request.EmployerEtfRate,
            MinimumWageForEpf = request.MinimumWageForEpf,
            MaximumEarningForEpf = request.MaximumEarningForEpf,
            MaximumEarningForEtf = request.MaximumEarningForEtf,
            IsDefault = request.IsDefault,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        await _dbContext.EpfEtfRuleSets.AddAsync(ruleSet);
        await _dbContext.SaveChangesAsync();

        return MapToDto(ruleSet);
    }

    public async Task UpdateAsync(Guid id, UpdateEpfEtfRuleSetRequest request)
    {
        var ruleSet = await _dbContext.EpfEtfRuleSets.FirstOrDefaultAsync(r => r.Id == id);
        if (ruleSet is null)
        {
            throw new KeyNotFoundException("EPF/ETF rule set not found");
        }

        var newEffectiveFrom = request.EffectiveFrom.HasValue
            ? DateOnly.FromDateTime(request.EffectiveFrom.Value)
            : ruleSet.EffectiveFrom;
        var newEffectiveTo = request.EffectiveTo.HasValue
            ? DateOnly.FromDateTime(request.EffectiveTo.Value)
            : ruleSet.EffectiveTo;

        ValidateDates(newEffectiveFrom.ToDateTime(TimeOnly.MinValue), newEffectiveTo?.ToDateTime(TimeOnly.MinValue));

        if (request.EmployeeEpfRate.HasValue || request.EmployerEpfRate.HasValue || request.EmployerEtfRate.HasValue)
        {
            ValidateRates(
                request.EmployeeEpfRate ?? ruleSet.EmployeeEpfRate,
                request.EmployerEpfRate ?? ruleSet.EmployerEpfRate,
                request.EmployerEtfRate ?? ruleSet.EmployerEtfRate);
        }

        if (request.MinimumWageForEpf.HasValue || request.MaximumEarningForEpf.HasValue || request.MaximumEarningForEtf.HasValue)
        {
            ValidateLimits(
                request.MinimumWageForEpf ?? ruleSet.MinimumWageForEpf,
                request.MaximumEarningForEpf ?? ruleSet.MaximumEarningForEpf,
                request.MaximumEarningForEtf ?? ruleSet.MaximumEarningForEtf);
        }

        if (request.Name != null)
        {
            ruleSet.Name = request.Name.Trim();
        }

        ruleSet.EffectiveFrom = newEffectiveFrom;
        ruleSet.EffectiveTo = newEffectiveTo;

        if (request.EmployeeEpfRate.HasValue)
        {
            ruleSet.EmployeeEpfRate = request.EmployeeEpfRate.Value;
        }

        if (request.EmployerEpfRate.HasValue)
        {
            ruleSet.EmployerEpfRate = request.EmployerEpfRate.Value;
        }

        if (request.EmployerEtfRate.HasValue)
        {
            ruleSet.EmployerEtfRate = request.EmployerEtfRate.Value;
        }

        if (request.MinimumWageForEpf.HasValue)
        {
            ruleSet.MinimumWageForEpf = request.MinimumWageForEpf.Value;
        }

        if (request.MaximumEarningForEpf.HasValue)
        {
            ruleSet.MaximumEarningForEpf = request.MaximumEarningForEpf.Value;
        }

        if (request.MaximumEarningForEtf.HasValue)
        {
            ruleSet.MaximumEarningForEtf = request.MaximumEarningForEtf.Value;
        }

        if (request.IsActive.HasValue)
        {
            ruleSet.IsActive = request.IsActive.Value;
        }

        if (request.IsDefault.HasValue && request.IsDefault.Value)
        {
            await ClearDefaultEpfEtfRuleSetsAsync(ruleSet.Id);
            ruleSet.IsDefault = true;
        }
        else if (request.IsDefault.HasValue)
        {
            ruleSet.IsDefault = request.IsDefault.Value;
        }

        ruleSet.ModifiedAt = DateTime.UtcNow;
        ruleSet.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private async Task ClearDefaultEpfEtfRuleSetsAsync(Guid? excludeId = null)
    {
        var defaults = await _dbContext.EpfEtfRuleSets
            .Where(r => r.IsDefault && (!excludeId.HasValue || r.Id != excludeId.Value))
            .ToListAsync();

        foreach (var ruleSet in defaults)
        {
            ruleSet.IsDefault = false;
        }
    }

    private static void ValidateDates(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }
    }

    private static void ValidateRates(decimal employeeRate, decimal employerRate, decimal employerEtfRate)
    {
        if (employeeRate < 0 || employerRate < 0 || employerEtfRate < 0)
        {
            throw new InvalidOperationException("Rates must be non-negative.");
        }
    }

    private static void ValidateLimits(decimal? minimumWageForEpf, decimal? maximumEarningForEpf, decimal? maximumEarningForEtf)
    {
        if (minimumWageForEpf.HasValue && minimumWageForEpf.Value < 0)
        {
            throw new InvalidOperationException("Minimum wage for EPF must be non-negative.");
        }

        if (maximumEarningForEpf.HasValue && maximumEarningForEpf.Value < 0)
        {
            throw new InvalidOperationException("Maximum earning for EPF must be non-negative.");
        }

        if (maximumEarningForEtf.HasValue && maximumEarningForEtf.Value < 0)
        {
            throw new InvalidOperationException("Maximum earning for ETF must be non-negative.");
        }
    }

    private static EpfEtfRuleSetDto MapToDto(EpfEtfRuleSet ruleSet)
    {
        return new EpfEtfRuleSetDto
        {
            Id = ruleSet.Id,
            Name = ruleSet.Name,
            EffectiveFrom = ruleSet.EffectiveFrom.ToDateTime(TimeOnly.MinValue),
            EffectiveTo = ruleSet.EffectiveTo?.ToDateTime(TimeOnly.MinValue),
            EmployeeEpfRate = ruleSet.EmployeeEpfRate,
            EmployerEpfRate = ruleSet.EmployerEpfRate,
            EmployerEtfRate = ruleSet.EmployerEtfRate,
            MinimumWageForEpf = ruleSet.MinimumWageForEpf,
            MaximumEarningForEpf = ruleSet.MaximumEarningForEpf,
            MaximumEarningForEtf = ruleSet.MaximumEarningForEtf,
            IsDefault = ruleSet.IsDefault,
            IsActive = ruleSet.IsActive
        };
    }
}
