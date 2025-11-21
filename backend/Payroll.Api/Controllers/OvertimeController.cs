using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Overtime;
using Payroll.Application.Overtime.DTOs;
using Payroll.Domain.Overtime;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeService _service;

    public OvertimeController(IOvertimeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] OvertimeStatus? status = null)
    {
        var result = await _service.GetAsync(page, pageSize, employeeId, date, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var record = await _service.GetByIdAsync(id);
        if (record == null)
        {
            return NotFound();
        }

        return Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOvertimeRecordRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOvertimeRecordRequest request)
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
