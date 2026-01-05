using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/statutory-reports")]
public class StatutoryReportsController : ControllerBase
{
    private readonly IStatutoryReportService _service;

    public StatutoryReportsController(IStatutoryReportService service)
    {
        _service = service;
    }

    [HttpPost("epf-etf")]
    public async Task<IActionResult> GenerateEpfEtfReport(
        [FromBody] EpfEtfReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var report = await _service.GenerateEpfEtfReportAsync(request, cancellationToken);
        return Ok(report);
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] StatutoryReportType? type,
        [FromQuery] Guid? payRunId,
        CancellationToken cancellationToken = default)
    {
        var reports = await _service.GetReportsAsync(type, payRunId, cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadReport(
        Guid id,
        [FromQuery] bool warnings = false,
        CancellationToken cancellationToken = default)
    {
        var file = await _service.DownloadReportAsync(id, warnings, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }
}
