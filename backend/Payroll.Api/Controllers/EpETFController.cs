using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpETFController : ControllerBase
{
    [HttpGet("rates")]
    public IActionResult GetRates() => Ok(new { EmployeeRate = 0.08, EmployerRate = 0.12 });
}
