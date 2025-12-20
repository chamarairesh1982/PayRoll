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
            .Include(r => r.Reliefs)
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
            .Include(r => r.Reliefs)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return ruleSet is null ? null : MapToDto(ruleSet);
    }

    public async Task<TaxRuleSetDto?> GetActiveRuleForDateAsync(DateOnly payDate)
    {
        var ruleSet = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .Include(r => r.Reliefs)
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
        ValidateSlabs(request.Slabs.Select(s => (s.FromAmount, s.ToAmount, s.Rate, s.Order)));
        ValidateReliefs(request.Reliefs.Select(r => (r.Name, r.Amount)));
        await ValidateNoOverlapAsync(
            request.EffectiveFrom,
            request.EffectiveTo,
            request.IsActive,
            null);

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
            Frequency = request.Frequency,
            IsActive = request.IsActive,
            CreatedBy = _currentUserService.UserName ?? "system",
            Slabs = request.Slabs.Select(s => new TaxSlab
            {
                Id = Guid.NewGuid(),
                FromAmount = s.FromAmount,
                ToAmount = s.ToAmount,
                Rate = s.Rate,
                Order = s.Order,
                CreatedBy = _currentUserService.UserName ?? "system"
            }).ToList(),
            Reliefs = request.Reliefs.Select(r => new TaxRelief
            {
                Id = Guid.NewGuid(),
                Name = r.Name.Trim(),
                Amount = r.Amount,
                ReliefType = r.ReliefType,
                Frequency = r.Frequency,
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
            .Include(r => r.Reliefs)
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
            ValidateSlabs(request.Slabs.Select(s => (s.FromAmount, s.ToAmount, s.Rate, s.Order)));
        }

        if (request.Reliefs != null)
        {
            ValidateReliefs(request.Reliefs.Select(r => (r.Name, r.Amount)));
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

        if (request.Frequency.HasValue)
        {
            ruleSet.Frequency = request.Frequency.Value;
        }

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
                Rate = s.Rate,
                Order = s.Order,
                CreatedBy = _currentUserService.UserName ?? "system"
            }).ToList();
        }

        if (request.Reliefs != null)
        {
            _dbContext.TaxReliefs.RemoveRange(ruleSet.Reliefs);
            ruleSet.Reliefs = request.Reliefs.Select(r => new TaxRelief
            {
                Id = r.Id ?? Guid.NewGuid(),
                TaxRuleSetId = ruleSet.Id,
                Name = r.Name.Trim(),
                Amount = r.Amount,
                ReliefType = r.ReliefType,
                Frequency = r.Frequency,
                CreatedBy = _currentUserService.UserName ?? "system"
            }).ToList();
        }

        ruleSet.ModifiedAt = DateTime.UtcNow;
        ruleSet.ModifiedBy = _currentUserService.UserName ?? "system";

        await ValidateNoOverlapAsync(
            ruleSet.EffectiveFrom.ToDateTime(TimeOnly.MinValue),
            ruleSet.EffectiveTo?.ToDateTime(TimeOnly.MinValue),
            ruleSet.IsActive,
            ruleSet.Id);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TaxSlabDto>> GetSlabsAsync(Guid slabSetId)
    {
        var slabs = await _dbContext.TaxSlabs
            .Where(s => s.TaxRuleSetId == slabSetId)
            .OrderBy(s => s.Order)
            .ThenBy(s => s.FromAmount)
            .AsNoTracking()
            .ToListAsync();

        return slabs.Select(s => new TaxSlabDto
        {
            Id = s.Id,
            FromAmount = s.FromAmount,
            ToAmount = s.ToAmount,
            Rate = s.Rate,
            Order = s.Order
        }).ToList();
    }

    public async Task<TaxSlabDto> AddSlabAsync(Guid slabSetId, CreateTaxSlabRequest request)
    {
        var ruleSet = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .FirstOrDefaultAsync(r => r.Id == slabSetId);
        if (ruleSet is null)
        {
            throw new KeyNotFoundException("Tax slab set not found.");
        }

        var slab = new TaxSlab
        {
            Id = Guid.NewGuid(),
            TaxRuleSetId = slabSetId,
            FromAmount = request.FromAmount,
            ToAmount = request.ToAmount,
            Rate = request.Rate,
            Order = request.Order,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        var candidateSlabs = ruleSet.Slabs
            .Select(s => (s.FromAmount, s.ToAmount, s.Rate, s.Order))
            .Append((slab.FromAmount, slab.ToAmount, slab.Rate, slab.Order));
        ValidateSlabs(candidateSlabs);

        ruleSet.Slabs.Add(slab);
        await _dbContext.SaveChangesAsync();

        return new TaxSlabDto
        {
            Id = slab.Id,
            FromAmount = slab.FromAmount,
            ToAmount = slab.ToAmount,
            Rate = slab.Rate,
            Order = slab.Order
        };
    }

    public async Task UpdateSlabAsync(Guid slabId, UpdateTaxSlabRequest request)
    {
        var slab = await _dbContext.TaxSlabs.FirstOrDefaultAsync(s => s.Id == slabId);
        if (slab is null)
        {
            throw new KeyNotFoundException("Tax slab not found.");
        }

        slab.FromAmount = request.FromAmount;
        slab.ToAmount = request.ToAmount;
        slab.Rate = request.Rate;
        slab.Order = request.Order;
        slab.ModifiedAt = DateTime.UtcNow;
        slab.ModifiedBy = _currentUserService.UserName ?? "system";

        var slabs = await _dbContext.TaxSlabs
            .Where(s => s.TaxRuleSetId == slab.TaxRuleSetId)
            .AsNoTracking()
            .ToListAsync();

        var candidateSlabs = slabs
            .Where(s => s.Id != slabId)
            .Select(s => (s.FromAmount, s.ToAmount, s.Rate, s.Order))
            .Append((request.FromAmount, request.ToAmount, request.Rate, request.Order));

        ValidateSlabs(candidateSlabs);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteSlabAsync(Guid slabId)
    {
        var slab = await _dbContext.TaxSlabs.FirstOrDefaultAsync(s => s.Id == slabId);
        if (slab is null)
        {
            return;
        }

        _dbContext.TaxSlabs.Remove(slab);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TaxReliefDto>> GetReliefsAsync(Guid slabSetId)
    {
        var reliefs = await _dbContext.TaxReliefs
            .Where(r => r.TaxRuleSetId == slabSetId)
            .OrderBy(r => r.Name)
            .AsNoTracking()
            .ToListAsync();

        return reliefs.Select(r => new TaxReliefDto
        {
            Id = r.Id,
            Name = r.Name,
            Amount = r.Amount,
            ReliefType = r.ReliefType,
            Frequency = r.Frequency
        }).ToList();
    }

    public async Task<TaxReliefDto> AddReliefAsync(Guid slabSetId, CreateTaxReliefRequest request)
    {
        var ruleSetExists = await _dbContext.TaxRuleSets.AnyAsync(r => r.Id == slabSetId);
        if (!ruleSetExists)
        {
            throw new KeyNotFoundException("Tax slab set not found.");
        }

        var relief = new TaxRelief
        {
            Id = Guid.NewGuid(),
            TaxRuleSetId = slabSetId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            ReliefType = request.ReliefType,
            Frequency = request.Frequency,
            CreatedBy = _currentUserService.UserName ?? "system"
        };

        ValidateReliefs(new[] { (relief.Name, relief.Amount) });

        await _dbContext.TaxReliefs.AddAsync(relief);
        await _dbContext.SaveChangesAsync();

        return new TaxReliefDto
        {
            Id = relief.Id,
            Name = relief.Name,
            Amount = relief.Amount,
            ReliefType = relief.ReliefType,
            Frequency = relief.Frequency
        };
    }

    public async Task UpdateReliefAsync(Guid reliefId, UpdateTaxReliefRequest request)
    {
        var relief = await _dbContext.TaxReliefs.FirstOrDefaultAsync(r => r.Id == reliefId);
        if (relief is null)
        {
            throw new KeyNotFoundException("Tax relief not found.");
        }

        relief.Name = request.Name.Trim();
        relief.Amount = request.Amount;
        relief.ReliefType = request.ReliefType;
        relief.Frequency = request.Frequency;
        relief.ModifiedAt = DateTime.UtcNow;
        relief.ModifiedBy = _currentUserService.UserName ?? "system";

        ValidateReliefs(new[] { (relief.Name, relief.Amount) });

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteReliefAsync(Guid reliefId)
    {
        var relief = await _dbContext.TaxReliefs.FirstOrDefaultAsync(r => r.Id == reliefId);
        if (relief is null)
        {
            return;
        }

        _dbContext.TaxReliefs.Remove(relief);
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

    private static void ValidateSlabs(IEnumerable<(decimal FromAmount, decimal? ToAmount, decimal Rate, int Order)> slabs)
    {
        var ordered = slabs
            .OrderBy(s => s.FromAmount)
            .ThenBy(s => s.ToAmount ?? decimal.MaxValue)
            .ToList();

        decimal? previousUpperBound = null;
        var hasPrevious = false;
        var first = true;

        foreach (var slab in ordered)
        {
            if (slab.FromAmount < 0 || slab.Rate < 0 || slab.Rate > 1)
            {
                throw new InvalidOperationException("Tax slab amounts and rates must be between 0 and 1.");
            }

            if (slab.ToAmount.HasValue && slab.ToAmount.Value <= slab.FromAmount)
            {
                throw new InvalidOperationException("Tax slab ToAmount must be greater than FromAmount when provided.");
            }

            if (first)
            {
                if (slab.FromAmount != 0)
                {
                    throw new InvalidOperationException("The first tax slab must start from 0.");
                }

                first = false;
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

    private static void ValidateReliefs(IEnumerable<(string Name, decimal Amount)> reliefs)
    {
        foreach (var relief in reliefs)
        {
            if (string.IsNullOrWhiteSpace(relief.Name))
            {
                throw new InvalidOperationException("Relief name is required.");
            }

            if (relief.Amount < 0)
            {
                throw new InvalidOperationException("Relief amounts must be non-negative.");
            }
        }
    }

    public async Task<IReadOnlyDictionary<Guid, TaxRuleSetDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, TaxRuleSetDto>();
        }

        var ruleSets = await _dbContext.TaxRuleSets
            .Include(r => r.Slabs)
            .Include(r => r.Reliefs)
            .AsNoTracking()
            .Where(r => idList.Contains(r.Id))
            .ToListAsync();

        return ruleSets.ToDictionary(r => r.Id, MapToDto);
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
            Frequency = ruleSet.Frequency,
            Slabs = ruleSet.Slabs
                .OrderBy(s => s.Order)
                .ThenBy(s => s.FromAmount)
                .Select(s => new TaxSlabDto
                {
                    Id = s.Id,
                    FromAmount = s.FromAmount,
                    ToAmount = s.ToAmount,
                    Rate = s.Rate,
                    Order = s.Order
                })
                .ToList(),
            Reliefs = ruleSet.Reliefs
                .OrderBy(r => r.Name)
                .Select(r => new TaxReliefDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Amount = r.Amount,
                    ReliefType = r.ReliefType,
                    Frequency = r.Frequency
                })
                .ToList()
        };
    }

    private async Task ValidateNoOverlapAsync(
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        bool isActive,
        Guid? excludeId)
    {
        if (!isActive)
        {
            return;
        }

        var fromDate = DateOnly.FromDateTime(effectiveFrom);
        var toDate = effectiveTo.HasValue ? DateOnly.FromDateTime(effectiveTo.Value) : (DateOnly?)null;

        var query = _dbContext.TaxRuleSets
            .AsNoTracking()
            .Where(r => r.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var overlaps = await query.AnyAsync(r =>
            r.EffectiveFrom <= (toDate ?? DateOnly.MaxValue)
            && (r.EffectiveTo ?? DateOnly.MaxValue) >= fromDate);

        if (overlaps)
        {
            throw new InvalidOperationException("An active tax slab set already exists for the selected effective date range.");
        }
    }
}
