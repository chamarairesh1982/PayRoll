using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public TaxController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("config")]
    public IActionResult GetConfig() => Ok(new { TaxFreeThreshold = 100000, PersonalRelief = 1200000 });

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] TaxPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.PreviewTaxAsync(request, cancellationToken);
        return Ok(result);
    }
}
