using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var payRuns = await _payrollService.GetPayRunsAsync(page, pageSize);
        return Ok(payRuns);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var payRun = await _payrollService.GetPayRunAsync(id);
        return payRun is null ? NotFound() : Ok(payRun);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RunPayrollRequest request)
    {
        var dto = new PayRunDto
        {
            Reference = request.Reference,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd
        };
        var created = await _payrollService.CreatePayRunAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _payrollService.ApprovePayRunAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/recalculate")]
    public async Task<IActionResult> Recalculate(Guid id)
    {
        var payRun = await _payrollService.RecalculatePayRunAsync(id);
        return Ok(payRun);
    }

    [HttpGet("payslips/{id:guid}")]
    public async Task<IActionResult> GetPaySlip(Guid id)
    {
        var paySlip = await _payrollService.GetPaySlipAsync(id);
        return paySlip is null ? NotFound() : Ok(paySlip);
    }
}

public record RunPayrollRequest(string Reference, DateTime PeriodStart, DateTime PeriodEnd);
