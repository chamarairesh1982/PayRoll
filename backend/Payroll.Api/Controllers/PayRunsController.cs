using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/payruns")]
public class PayRunsController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly IPayslipDocumentService _payslipDocumentService;

    public PayRunsController(IPayrollService payrollService, IPayslipDocumentService payslipDocumentService)
    {
        _payrollService = payrollService;
        _payslipDocumentService = payslipDocumentService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PayRunQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.GetPayRunsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await _payrollService.GetPayRunAsync(id, cancellationToken);
        return payRun is null ? NotFound() : Ok(payRun);
    }

    [HttpGet("{id:guid}/time-reconciliation")]
    public async Task<IActionResult> GetTimeReconciliation(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.GetTimeReconciliationAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        var created = await _payrollService.CreatePayRunAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/prepare")]
    public async Task<IActionResult> Prepare(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await _payrollService.PreparePayRunAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:guid}/recalculate")]
    public async Task<IActionResult> Recalculate(Guid id, [FromBody] RecalculatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.RecalculatePayRunAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await _payrollService.ApprovePayRunAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await _payrollService.LockPayRunAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.UnlockPayRunAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangePayRunStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.ChangeStatusAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/general-ledger/generate")]
    public async Task<IActionResult> GenerateGeneralLedger(Guid id, CancellationToken cancellationToken = default)
    {
        var export = await _payrollService.GenerateGeneralLedgerExportAsync(id, cancellationToken);
        return Ok(export);
    }

    [HttpPost("{id:guid}/general-ledger/review")]
    public async Task<IActionResult> ReviewGeneralLedger(Guid id, [FromBody] GeneralLedgerActionRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.ReviewGeneralLedgerExportAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/general-ledger/approve")]
    public async Task<IActionResult> ApproveGeneralLedger(Guid id, [FromBody] GeneralLedgerActionRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.ApproveGeneralLedgerExportAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/general-ledger/export")]
    public async Task<IActionResult> ExportGeneralLedger(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _payrollService.ExportGeneralLedgerAsync(id, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpGet("{id:guid}/apit-report")]
    public async Task<IActionResult> GetApitReport(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _payrollService.GetApitReportAsync(id, cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}/apit-certificate")]
    public async Task<IActionResult> GetApitCertificate(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var file = await _payrollService.GenerateApitCertificateAsync(payRunId, paySlipId, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}")]
    public async Task<IActionResult> GetPaySlip(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var paySlip = await _payrollService.GetPaySlipAsync(payRunId, paySlipId, cancellationToken);
        return paySlip is null ? NotFound() : Ok(paySlip);
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}/export")]
    public async Task<IActionResult> ExportPaySlip(Guid payRunId, Guid paySlipId, [FromQuery] string format = "pdf", CancellationToken cancellationToken = default)
    {
        var file = await _payrollService.ExportPaySlipAsync(payRunId, paySlipId, format, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpPost("{payRunId:guid}/payslips/{employeeId:guid}/generate")]
    public async Task<IActionResult> GeneratePayslipDocument(
        Guid payRunId,
        Guid employeeId,
        [FromQuery] bool regenerate = false,
        CancellationToken cancellationToken = default)
    {
        var document = await _payslipDocumentService.GenerateAsync(payRunId, employeeId, regenerate, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost("{payRunId:guid}/payslips/generate-bulk")]
    public async Task<IActionResult> GeneratePayslipDocumentsBulk(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var result = await _payslipDocumentService.GenerateBulkAsync(payRunId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{payRunId:guid}/payslips/documents")]
    public async Task<IActionResult> GetPayslipDocuments(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var documents = await _payslipDocumentService.GetDocumentsForPayRunAsync(payRunId, cancellationToken);
        return Ok(documents);
    }
}
