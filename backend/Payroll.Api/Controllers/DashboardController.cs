using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? costCenterId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DashboardSummaryRequest(periodStart, periodEnd, companyId, branchId, costCenterId);
        var summary = await _dashboardService.GetSummaryAsync(request, cancellationToken);
        return Ok(summary);
    }
}
