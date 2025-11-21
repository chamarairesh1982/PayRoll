using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxController : ControllerBase
{
    [HttpGet("config")]
    public IActionResult GetConfig() => Ok(new { TaxFreeThreshold = 100000, PersonalRelief = 1200000 });
}
