using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api")]
public class BankExportsController : ControllerBase
{
    private readonly IBankExportService _bankExportService;

    public BankExportsController(IBankExportService bankExportService)
    {
        _bankExportService = bankExportService;
    }

    [HttpGet("bank-exports/templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken = default)
    {
        var templates = await _bankExportService.GetTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    [HttpGet("payruns/{payRunId:guid}/bank-exports")]
    public async Task<IActionResult> GetExports(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var exports = await _bankExportService.GetExportsAsync(payRunId, cancellationToken);
        return Ok(exports);
    }

    [HttpPost("payruns/{payRunId:guid}/bank-exports")]
    public async Task<IActionResult> GenerateExport(
        Guid payRunId,
        [FromBody] BankExportGenerateRequest request,
        [FromQuery] bool regenerate = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _bankExportService.GenerateExportAsync(payRunId, request, regenerate, cancellationToken);
        return Ok(result);
    }

    [HttpGet("bank-exports/{exportId:guid}/download")]
    public async Task<IActionResult> DownloadExport(Guid exportId, CancellationToken cancellationToken = default)
    {
        var file = await _bankExportService.DownloadExportAsync(exportId, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpGet("bank-exports/{exportId:guid}/errors")]
    public async Task<IActionResult> DownloadErrors(Guid exportId, CancellationToken cancellationToken = default)
    {
        var file = await _bankExportService.DownloadErrorsAsync(exportId, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }
}
