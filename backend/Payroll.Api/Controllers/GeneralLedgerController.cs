using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/general-ledger")]
public class GeneralLedgerController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public GeneralLedgerController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("mappings")]
    public async Task<IActionResult> GetMappings(CancellationToken cancellationToken = default)
    {
        var mappings = await _payrollService.GetGeneralLedgerAccountMappingsAsync(cancellationToken);
        return Ok(mappings);
    }

    [HttpPost("mappings")]
    public async Task<IActionResult> UpsertMapping([FromBody] UpsertGeneralLedgerAccountMappingRequest request, CancellationToken cancellationToken = default)
    {
        var mapping = await _payrollService.UpsertGeneralLedgerAccountMappingAsync(request, cancellationToken);
        return Ok(mapping);
    }
}
