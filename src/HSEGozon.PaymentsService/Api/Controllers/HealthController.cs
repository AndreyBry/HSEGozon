using Microsoft.AspNetCore.Mvc;

namespace HSEGozon.PaymentsService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "PaymentsService" });
    }
}

