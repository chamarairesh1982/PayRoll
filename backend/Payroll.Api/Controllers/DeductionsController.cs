using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeductionsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(Array.Empty<object>());
}
