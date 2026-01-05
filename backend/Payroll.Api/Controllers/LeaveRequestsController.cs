using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Leave;
using Payroll.Application.Leave.DTOs;
using Payroll.Domain.Leave;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/leave-requests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _service;

    public LeaveRequestsController(ILeaveRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] LeaveStatus? status = null)
    {
        var result = await _service.GetAsync(page, pageSize, employeeId, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var leave = await _service.GetByIdAsync(id);
        if (leave == null)
        {
            return NotFound();
        }

        return Ok(leave);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestRequest request)
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
