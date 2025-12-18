using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("statutory")]
    public async Task<IActionResult> GetStatutoryReport(
        [FromQuery] DateOnly period,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? costCenterId,
        [FromQuery] bool? isConsolidated,
        CancellationToken cancellationToken)
    {
        var file = await _reportService.GenerateStatutoryReportAsync(
            period,
            companyId,
            branchId,
            costCenterId,
            isConsolidated,
            cancellationToken);
        return File(file, "application/octet-stream", "statutory-report.bin");
    }
}
