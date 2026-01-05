using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/gl")]
public class GeneralLedgerController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public GeneralLedgerController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken = default)
    {
        var accounts = await _payrollService.GetGlAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> UpsertAccount([FromBody] UpsertGlAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _payrollService.UpsertGlAccountAsync(request, cancellationToken);
        return Ok(account);
    }

    [HttpDelete("accounts/{accountId:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid accountId, CancellationToken cancellationToken = default)
    {
        await _payrollService.DeleteGlAccountAsync(accountId, cancellationToken);
        return NoContent();
    }

    [HttpGet("mappings")]
    public async Task<IActionResult> GetMappings(CancellationToken cancellationToken = default)
    {
        var mappings = await _payrollService.GetGlMappingsAsync(cancellationToken);
        return Ok(mappings);
    }

    [HttpPost("mappings")]
    public async Task<IActionResult> UpsertMapping([FromBody] UpsertGlMappingRequest request, CancellationToken cancellationToken = default)
    {
        var mapping = await _payrollService.UpsertGlMappingAsync(request, cancellationToken);
        return Ok(mapping);
    }

    [HttpGet("batches/{batchId:guid}")]
    public async Task<IActionResult> GetBatch(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _payrollService.GetGlJournalBatchAsync(batchId, cancellationToken);
        return batch is null ? NotFound() : Ok(batch);
    }

    [HttpPost("batches/{batchId:guid}/approve")]
    public async Task<IActionResult> ApproveBatch(Guid batchId, [FromBody] GlJournalBatchActionRequest request, CancellationToken cancellationToken = default)
    {
        var batch = await _payrollService.ApproveGlJournalBatchAsync(batchId, request, cancellationToken);
        return Ok(batch);
    }

    [HttpGet("batches/{batchId:guid}/export")]
    public async Task<IActionResult> ExportBatch(Guid batchId, [FromQuery] string format = "csv", CancellationToken cancellationToken = default)
    {
        var file = await _payrollService.ExportGlJournalBatchAsync(batchId, format, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }
}
