using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Scheduled.API.Models;
using Scheduled.API.Services;
using System.Diagnostics.Metrics;

namespace Scheduled.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriversController : ControllerBase
    {
        private static List<Driver> drivers = new List<Driver>();

        private readonly ILogger<DriversController> _logger;

        public DriversController(ILogger<DriversController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetDrivers()
        {
            var items = drivers.Where(x => x.Status == 1).ToList();
            return Ok(items);
        }

        [HttpPost]
        public IActionResult CreateDriver(Driver data)
        {
            // Fire-and-forget jobs are executed only once and almost immediately after creation.
            BackgroundJob.Enqueue<IServiceManagement>(x => x.SendEmail());

            if (ModelState.IsValid)
            {
                drivers.Add(data);

                return CreatedAtAction("GetDriver", new { data.Id }, data);
            }

            return new JsonResult("Something went wrong") { StatusCode = 500 };
        }

        [HttpGet("{id}")]
        public IActionResult GetDriver(Guid id)
        {
            // Delayed jobs are executed only once too, but not immediately, after a certain time interval.
            BackgroundJob.Schedule<IServiceManagement>(x => x.UpdateDatabase(), TimeSpan.FromSeconds(20));

            var item = drivers.FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDriver(Guid id, Driver item)
        {
            if (id != item.Id)
                return BadRequest();

            var existItem = drivers.FirstOrDefault(x => x.Id == id);

            if (existItem == null)
                return NotFound();

            existItem.Name = item.Name;
            existItem.DriverNumber = item.DriverNumber;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDriver(Guid id)
        {
            var existItem = drivers.FirstOrDefault(x => x.Id == id);

            if (existItem == null)
                return NotFound();

            existItem.Status = 0;

            return Ok(existItem);
        }
    }
}