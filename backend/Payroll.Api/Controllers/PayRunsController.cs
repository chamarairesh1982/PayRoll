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

    public PayRunsController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        var created = await _payrollService.CreatePayRunAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/prepare")]
    public async Task<IActionResult> Prepare(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.PreparePayRunAsync(id, request, cancellationToken);
        return NoContent();
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
        await _payrollService.ApprovePayRunAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, [FromBody] PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.LockPayRunAsync(id, request, cancellationToken);
        return NoContent();
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

    [HttpPost("{id:guid}/bank-export")]
    public async Task<IActionResult> GenerateBankExport(Guid id, [FromBody] BankExportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.GenerateBankExportAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/bank-export/downloaded")]
    public async Task<IActionResult> MarkBankExportDownloaded(Guid id, CancellationToken cancellationToken = default)
    {
        await _payrollService.MarkBankExportDownloadedAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}")]
    public async Task<IActionResult> GetPaySlip(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var paySlip = await _payrollService.GetPaySlipAsync(payRunId, paySlipId, cancellationToken);
        return paySlip is null ? NotFound() : Ok(paySlip);
    }
}
