using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs.Organizations;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IPayrollDbContext _dbContext;

    public OrganizationService(IPayrollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);

        return companies;
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid? companyId = null, CancellationToken cancellationToken = default)
    {
        var branches = _dbContext.Branches.AsNoTracking();

        if (companyId.HasValue)
        {
            branches = branches.Where(b => b.CompanyId == companyId.Value);
        }

        var results = await branches
            .OrderBy(b => b.Code)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                CompanyId = b.CompanyId
            })
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<IReadOnlyList<CostCenterDto>> GetCostCentersAsync(
        Guid? companyId = null,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var costCenters = _dbContext.CostCenters.AsNoTracking();

        if (companyId.HasValue)
        {
            costCenters = costCenters.Where(cc =>
                (cc.CompanyId.HasValue && cc.CompanyId == companyId.Value)
                || (cc.Branch != null && cc.Branch.CompanyId == companyId.Value));
        }

        if (branchId.HasValue)
        {
            costCenters = costCenters.Where(cc => cc.BranchId == branchId.Value);
        }

        var results = await costCenters
            .OrderBy(cc => cc.Code)
            .Select(cc => new CostCenterDto
            {
                Id = cc.Id,
                Code = cc.Code,
                Name = cc.Name,
                CompanyId = cc.CompanyId,
                BranchId = cc.BranchId
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
