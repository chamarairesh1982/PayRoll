using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/tax")]
public class TaxDocumentsController : ControllerBase
{
    private readonly ITaxDocumentService _service;

    public TaxDocumentsController(ITaxDocumentService service)
    {
        _service = service;
    }

    [HttpPost("reports/monthly")]
    public async Task<IActionResult> GenerateMonthlyReport(
        [FromBody] MonthlyTaxReportRequestDto request,
        [FromQuery] bool regenerate = false,
        CancellationToken cancellationToken = default)
    {
        var document = await _service.GenerateMonthlyReportAsync(request, regenerate, cancellationToken);
        return Ok(document);
    }

    [HttpPost("reports/annual")]
    public async Task<IActionResult> GenerateAnnualReport(
        [FromBody] AnnualTaxReportRequestDto request,
        [FromQuery] bool regenerate = false,
        CancellationToken cancellationToken = default)
    {
        var document = await _service.GenerateAnnualReportAsync(request, regenerate, cancellationToken);
        return Ok(document);
    }

    [HttpPost("certificates")]
    public async Task<IActionResult> GenerateCertificate(
        [FromBody] TaxCertificateRequestDto request,
        [FromQuery] bool regenerate = false,
        CancellationToken cancellationToken = default)
    {
        var document = await _service.GenerateEmployeeCertificateAsync(request, regenerate, cancellationToken);
        return Ok(document);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] GeneratedTaxDocumentType? type,
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _service.GetDocumentsAsync(type, year, employeeId, cancellationToken);
        return Ok(documents);
    }

    [HttpGet("documents/{id:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _service.DownloadAsync(id, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        return Ok(file);
    }
}
