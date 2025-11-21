using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var records = await _attendanceService.GetAttendanceAsync(page, pageSize);
        return Ok(records);
    }

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] AttendanceDto dto)
    {
        await _attendanceService.RecordAttendanceAsync(dto);
        return Accepted();
    }
}
