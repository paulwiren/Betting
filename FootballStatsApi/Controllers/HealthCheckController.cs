using Microsoft.AspNetCore.Mvc;

namespace FootballStatsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Content("OK");
        }
    }
}
