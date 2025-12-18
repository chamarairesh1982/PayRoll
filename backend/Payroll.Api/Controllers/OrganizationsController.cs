using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies(CancellationToken cancellationToken)
    {
        var companies = await _organizationService.GetCompaniesAsync(cancellationToken);
        return Ok(companies);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches([FromQuery] Guid? companyId, CancellationToken cancellationToken)
    {
        var branches = await _organizationService.GetBranchesAsync(companyId, cancellationToken);
        return Ok(branches);
    }

    [HttpGet("cost-centers")]
    public async Task<IActionResult> GetCostCenters(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken)
    {
        var costCenters = await _organizationService.GetCostCentersAsync(companyId, branchId, cancellationToken);
        return Ok(costCenters);
    }
}
