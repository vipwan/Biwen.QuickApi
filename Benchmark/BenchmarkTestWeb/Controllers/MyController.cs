using Microsoft.AspNetCore.Mvc;

namespace BenchmarkTestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Tags("API")]
    public class MyController : ControllerBase
    {
        //private readonly ILogger<WeatherForecastController> _logger;
        //public MyController(ILogger<WeatherForecastController> logger)
        //{
        //    _logger = logger;
        //}

        [HttpPost]
        public IActionResult Post([FromBody] MyRequest request)
        {
            var validResult = request.Validate();
            if (!validResult.IsValid)
            {
                return BadRequest();
            }
            return Ok(request);
        }
    }
}
