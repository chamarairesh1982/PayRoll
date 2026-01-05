using Microsoft.AspNetCore.Mvc;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/overtime/rules")]
public class OtController : ControllerBase
{
    private readonly IOvertimeRuleService _overtimeRuleService;

    public OtController(IOvertimeRuleService overtimeRuleService)
    {
        _overtimeRuleService = overtimeRuleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules(CancellationToken ct)
    {
        var rules = await _overtimeRuleService.GetAllAsync(ct);
        return Ok(rules);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRule(Guid id, CancellationToken ct)
    {
        var rule = await _overtimeRuleService.GetByIdAsync(id, ct);
        if (rule is null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] CreateOTRuleRequest request, CancellationToken ct)
    {
        var rule = await _overtimeRuleService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateOTRuleRequest request, CancellationToken ct)
    {
        var rule = await _overtimeRuleService.UpdateAsync(id, request, ct);
        return Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id, CancellationToken ct)
    {
        await _overtimeRuleService.DeleteAsync(id, ct);
        return NoContent();
    }
}
