using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;
using System;

namespace Payroll.Application.PayrollConfig;

public class TaxRuleSetService : ITaxRuleSetService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public TaxRuleSetService(IPayrollDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<TaxRuleSetDto>> GetAsync(int page, int pageSize, int? yearOfAssessment)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 1);

        var query = _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .AsNoTracking()
            .AsQueryable();

        if (yearOfAssessment.HasValue)
        {
            query = query.Where(r => r.YearOfAssessment == yearOfAssessment.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.YearOfAssessment)
            .ThenByDescending(r => r.EffectiveFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return new PaginatedResult<TaxRuleSetDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaxRuleSetDto?> GetByIdAsync(Guid id)
    {
        var ruleSet = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return ruleSet is null ? null : MapToDto(ruleSet);
    }

    public async Task<TaxRuleSetDto?> GetActiveRuleForDateAsync(DateOnly payDate)
    {
        var ruleSet = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .AsNoTracking()
            .Where(r => r.IsActive
                        && r.EffectiveFrom <= payDate
                        && (r.EffectiveTo == null || r.EffectiveTo >= payDate))
            .OrderByDescending(r => r.IsDefault)
            .ThenByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync();

        return ruleSet is null ? null : MapToDto(ruleSet);
    }

    public async Task<TaxRuleSetDto> CreateAsync(CreateTaxRuleSetRequest request)
    {
        ValidateRuleSetDates(request.EffectiveFrom, request.EffectiveTo);
        ValidateSlabs(request.Slabs.Select(s => (s.FromAmount, s.ToAmount, s.RatePercent, s.Order)));

        if (request.IsDefault)
        {
            await ClearDefaultTaxRuleSetsAsync();
        }

        var ruleSet = new TaxRuleSet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            YearOfAssessment = request.YearOfAssessment,
            EffectiveFrom = DateOnly.FromDateTime(request.EffectiveFrom),
            EffectiveTo = request.EffectiveTo.HasValue ? DateOnly.FromDateTime(request.EffectiveTo.Value) : null,
            IsDefault = request.IsDefault,
            CreatedBy = _currentUserService.UserName ?? "system",
            Slabs = request.Slabs.Select(s => new TaxSlab
            {
                Id = Guid.NewGuid(),
                FromAmount = s.FromAmount,
                ToAmount = s.ToAmount,
                RatePercent = s.RatePercent,
                Order = s.Order,
                CreatedBy = _currentUserService.UserName ?? "system"
            }).ToList()
        };

        await _dbContext.TaxRuleSets.AddAsync(ruleSet);
        await _dbContext.SaveChangesAsync();

        return MapToDto(ruleSet);
    }

    public async Task UpdateAsync(Guid id, UpdateTaxRuleSetRequest request)
    {
        var ruleSet = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (ruleSet is null)
        {
            throw new KeyNotFoundException("Tax rule set not found");
        }

        var newEffectiveFrom = request.EffectiveFrom.HasValue
            ? DateOnly.FromDateTime(request.EffectiveFrom.Value)
            : ruleSet.EffectiveFrom;
        var newEffectiveTo = request.EffectiveTo.HasValue
            ? DateOnly.FromDateTime(request.EffectiveTo.Value)
            : ruleSet.EffectiveTo;

        ValidateRuleSetDates(newEffectiveFrom.ToDateTime(TimeOnly.MinValue), newEffectiveTo?.ToDateTime(TimeOnly.MinValue));

        if (request.Slabs != null)
        {
            ValidateSlabs(request.Slabs.Select(s => (s.FromAmount, s.ToAmount, s.RatePercent, s.Order)));
        }

        if (request.Name != null)
        {
            ruleSet.Name = request.Name.Trim();
        }

        if (request.YearOfAssessment.HasValue)
        {
            ruleSet.YearOfAssessment = request.YearOfAssessment.Value;
        }

        ruleSet.EffectiveFrom = newEffectiveFrom;
        ruleSet.EffectiveTo = newEffectiveTo;

        if (request.IsActive.HasValue)
        {
            ruleSet.IsActive = request.IsActive.Value;
        }

        if (request.IsDefault.HasValue && request.IsDefault.Value)
        {
            await ClearDefaultTaxRuleSetsAsync(ruleSet.Id);
            ruleSet.IsDefault = true;
        }
        else if (request.IsDefault.HasValue)
        {
            ruleSet.IsDefault = request.IsDefault.Value;
        }

        if (request.Slabs != null)
        {
            _dbContext.TaxSlabs.RemoveRange(ruleSet.Slabs);
            ruleSet.Slabs = request.Slabs.Select(s => new TaxSlab
            {
                Id = s.Id ?? Guid.NewGuid(),
                TaxRuleSetId = ruleSet.Id,
                FromAmount = s.FromAmount,
                ToAmount = s.ToAmount,
                RatePercent = s.RatePercent,
                Order = s.Order,
                CreatedBy = _currentUserService.UserName ?? "system"
            }).ToList();
        }

        ruleSet.ModifiedAt = DateTime.UtcNow;
        ruleSet.ModifiedBy = _currentUserService.UserName ?? "system";

        await _dbContext.SaveChangesAsync();
    }

    private async Task ClearDefaultTaxRuleSetsAsync(Guid? excludeId = null)
    {
        var defaults = await _dbContext.TaxRuleSets
            .Where(r => r.IsDefault && (!excludeId.HasValue || r.Id != excludeId.Value))
            .ToListAsync();

        foreach (var ruleSet in defaults)
        {
            ruleSet.IsDefault = false;
        }

        // TODO: ensure only one default per period/year when overlapping ranges are introduced.
    }

    private static void ValidateRuleSetDates(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }
    }

    private static void ValidateSlabs(IEnumerable<(decimal FromAmount, decimal? ToAmount, decimal RatePercent, int Order)> slabs)
    {
        var ordered = slabs
            .OrderBy(s => s.FromAmount)
            .ThenBy(s => s.ToAmount ?? decimal.MaxValue)
            .ToList();

        decimal? previousUpperBound = null;
        var hasPrevious = false;

        foreach (var slab in ordered)
        {
            if (slab.FromAmount < 0 || slab.RatePercent < 0)
            {
                throw new InvalidOperationException("Tax slab amounts and rates must be non-negative.");
            }

            if (slab.ToAmount.HasValue && slab.ToAmount.Value <= slab.FromAmount)
            {
                throw new InvalidOperationException("Tax slab ToAmount must be greater than FromAmount when provided.");
            }

            if (hasPrevious)
            {
                if (!previousUpperBound.HasValue)
                {
                    throw new InvalidOperationException("An open-ended tax slab must be the last slab.");
                }

                if (slab.FromAmount < previousUpperBound.Value)
                {
                    throw new InvalidOperationException("Tax slabs must not overlap.");
                }
            }

            hasPrevious = true;
            previousUpperBound = slab.ToAmount;
        }
    }

    private static TaxRuleSetDto MapToDto(TaxRuleSet ruleSet)
    {
        return new TaxRuleSetDto
        {
            Id = ruleSet.Id,
            Name = ruleSet.Name,
            YearOfAssessment = ruleSet.YearOfAssessment,
            EffectiveFrom = ruleSet.EffectiveFrom.ToDateTime(TimeOnly.MinValue),
            EffectiveTo = ruleSet.EffectiveTo?.ToDateTime(TimeOnly.MinValue),
            IsDefault = ruleSet.IsDefault,
            IsActive = ruleSet.IsActive,
            Slabs = ruleSet.Slabs
                .OrderBy(s => s.Order)
                .ThenBy(s => s.FromAmount)
                .Select(s => new TaxSlabDto
                {
                    Id = s.Id,
                    FromAmount = s.FromAmount,
                    ToAmount = s.ToAmount,
                    RatePercent = s.RatePercent,
                    Order = s.Order
                })
                .ToList()
        };
    }
}
