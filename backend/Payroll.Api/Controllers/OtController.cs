using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtController : ControllerBase
{
    [HttpGet("config")]
    public IActionResult GetConfig() => Ok(new { StandardRateMultiplier = 1.5, WeekendRateMultiplier = 2.0 });
}
