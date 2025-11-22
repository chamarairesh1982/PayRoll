using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Payroll;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayRunsController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayRunsController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] PayRunStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.GetPayRunsAsync(page, pageSize, status, cancellationToken);
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

    [HttpPost("{id:guid}/recalculate")]
    public async Task<IActionResult> Recalculate(Guid id, [FromBody] RecalculatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.RecalculatePayRunAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangePayRunStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _payrollService.ChangeStatusAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}")]
    public async Task<IActionResult> GetPaySlip(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var paySlip = await _payrollService.GetPaySlipAsync(payRunId, paySlipId, cancellationToken);
        return paySlip is null ? NotFound() : Ok(paySlip);
    }
}
