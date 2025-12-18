using Microsoft.AspNetCore.Mvc;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtController : ControllerBase
{
    private readonly IOvertimeRuleService _overtimeRuleService;

    public OtController(IOvertimeRuleService overtimeRuleService)
    {
        _overtimeRuleService = overtimeRuleService;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _overtimeRuleService.GetAsync(ct);
        return Ok(config);
    }

    [HttpPut("config")]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateOvertimeRuleConfigRequest request, CancellationToken ct)
    {
        var config = await _overtimeRuleService.UpdateAsync(request, ct);
        return Ok(config);
    }
}
