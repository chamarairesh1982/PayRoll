using Payroll.Application.DTOs.Organizations;

namespace Payroll.Application.Interfaces;

public interface IOrganizationService
{
    Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CostCenterDto>> GetCostCentersAsync(
        Guid? companyId = null,
        Guid? branchId = null,
        CancellationToken cancellationToken = default);
}
