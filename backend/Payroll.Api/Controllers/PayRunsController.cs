using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Payroll;
using Payroll.Application.Payroll.Requests;
using Payroll.Domain.Payroll;

namespace Payroll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PayRunsController : ControllerBase
{
    private readonly IPayRunService _payRunService;

    public PayRunsController(IPayRunService payRunService)
    {
        _payRunService = payRunService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] PayRunStatus? status = null)
    {
        var result = await _payRunService.GetPayRunsAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _payRunService.GetPayRunAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayRunRequest request)
    {
        var created = await _payRunService.CreatePayRunAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/recalculate")]
    public async Task<IActionResult> Recalculate(Guid id, [FromBody] RecalculatePayRunRequest request)
    {
        await _payRunService.RecalculatePayRunAsync(id, request);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangePayRunStatusRequest request)
    {
        await _payRunService.ChangeStatusAsync(id, request);
        return NoContent();
    }

    [HttpGet("{payRunId:guid}/payslips/{paySlipId:guid}")]
    public async Task<IActionResult> GetPaySlip(Guid payRunId, Guid paySlipId)
    {
        var slip = await _payRunService.GetPaySlipAsync(payRunId, paySlipId);
        if (slip == null) return NotFound();
        return Ok(slip);
    }
}
