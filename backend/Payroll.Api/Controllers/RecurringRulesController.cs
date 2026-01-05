using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs.RecurringRules;
using Payroll.Application.RecurringRules;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/recurring-rules")]
public class RecurringRulesController : ControllerBase
{
    private readonly IRecurringRuleService _recurringRuleService;

    public RecurringRulesController(IRecurringRuleService recurringRuleService)
    {
        _recurringRuleService = recurringRuleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] bool? isActive = null)
    {
        var result = await _recurringRuleService.GetAsync(page, pageSize, isActive);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _recurringRuleService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringRuleRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _recurringRuleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecurringRuleRequestDto request, CancellationToken cancellationToken)
    {
        await _recurringRuleService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _recurringRuleService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] RecurringRuleSimulationRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _recurringRuleService.SimulateAsync(request, cancellationToken);
        return Ok(result);
    }
}
