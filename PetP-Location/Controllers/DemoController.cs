using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetP_Location.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;
        public DemoController(ILogger<DemoController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("GET request received at DemoController");

            try
            {

                _logger.LogDebug("Processing GET request in PetP-location");


                _logger.LogInformation("Successfully Location app");
                return Ok("Response Location");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while processing GET request");
                return StatusCode(500, "An error occurred");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("GET request for id: {Id} received", id);

            try
            {

                _logger.LogDebug("Processing GET request for id: {Id}", id);


                _logger.LogInformation("Successfully retrieved data for id: {Id}", id);
                return Ok($"Response for Location with ID: {id}");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while retrieving data for id: {Id}", id);
                return StatusCode(500, "An error occurred");
            }
        }
    }
}
