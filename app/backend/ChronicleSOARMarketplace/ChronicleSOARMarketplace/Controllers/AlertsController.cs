using ChronicleSOARMarketplace.Models;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ChronicleSOARMarketplace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AlertsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Alert>> GetAlerts()
        {
            Console.WriteLine("HERE");
            var alerts = new List<Alert>();
            var outputDir = _configuration["Output:Directory"];
            if (Directory.Exists(outputDir))
            {
                foreach (var file in Directory.GetFiles(outputDir, "*.json"))
                {
                    var json = System.IO.File.ReadAllText(file);
                    var alert = JsonSerializer.Deserialize<Alert>(json);
                    alerts.Add(alert);
                }
            }
            return Ok(alerts);
        }
    }
}
