using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck() => Ok(new { Status = "ok" });
}
