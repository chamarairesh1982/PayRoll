using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var leave = await _leaveService.GetLeaveRequestsAsync(page, pageSize);
        return Ok(leave);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var leave = await _leaveService.GetLeaveRequestAsync(id);
        return leave is null ? NotFound() : Ok(leave);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeaveDto dto)
    {
        var created = await _leaveService.CreateLeaveRequestAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _leaveService.ApproveLeaveAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _leaveService.RejectLeaveAsync(id);
        return NoContent();
    }
}
