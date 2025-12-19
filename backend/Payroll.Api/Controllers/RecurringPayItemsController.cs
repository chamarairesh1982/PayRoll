using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs.RecurringPayItems;
using Payroll.Application.RecurringPayItems;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/recurring-pay-items")]
public class RecurringPayItemsController : ControllerBase
{
    private readonly IRecurringPayItemService _service;

    public RecurringPayItemsController(IRecurringPayItemService service)
    {
        _service = service;
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetRulesAsync(page, pageSize, activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("rules/{id:guid}")]
    public async Task<IActionResult> GetRule(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetRuleByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule(
        [FromBody] CreateRecurringPayItemRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateRuleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRule), new { id = created.Id }, created);
    }

    [HttpPut("rules/{id:guid}")]
    public async Task<IActionResult> UpdateRule(
        Guid id,
        [FromBody] UpdateRecurringPayItemRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdateRuleAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("rules/{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteRuleAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? ruleId = null,
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAssignmentsAsync(employeeId, ruleId, activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpPost("assignments")]
    public async Task<IActionResult> CreateAssignments(
        [FromBody] CreateRecurringPayItemAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateAssignmentsAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPut("assignments/{id:guid}")]
    public async Task<IActionResult> UpdateAssignment(
        Guid id,
        [FromBody] UpdateRecurringPayItemAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdateAssignmentAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("assignments/{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAssignmentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate(
        [FromBody] RecurringPayItemSimulationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.SimulateAsync(request, cancellationToken);
        return Ok(result);
    }
}
