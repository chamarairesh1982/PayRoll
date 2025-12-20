using Microsoft.AspNetCore.Mvc;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/bank-branches")]
public class BankBranchesController : ControllerBase
{
    private readonly IBankBranchService _service;

    public BankBranchesController(IBankBranchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? bankId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _service.GetAsync(page, pageSize, bankId, search, isActive);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankBranchRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankBranchRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
