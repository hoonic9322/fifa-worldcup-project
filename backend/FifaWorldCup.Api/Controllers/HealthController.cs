using Microsoft.AspNetCore.Mvc;

namespace FifaWorldCup.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "OK",
                message = "FIFA World Cup API is running",
                time = DateTime.Now
            });
        }
    }
}