using Microsoft.AspNetCore.Mvc;

namespace HSEGozon.OrdersService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "OrdersService" });
    }
}

