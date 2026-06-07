using Microsoft.AspNetCore.Mvc;
namespace PaymentSwitch.API.Controllers;
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow, Version = "1.0.0" });
}