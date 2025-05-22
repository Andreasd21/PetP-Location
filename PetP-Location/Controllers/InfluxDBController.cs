using InfluxDB.Client.Core;
using Microsoft.AspNetCore.Mvc;
using PetP_Location.Service;

namespace PetP_Location.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfluxDBController : ControllerBase
    {
        private readonly InfluxDbService _influxDbService;

        public InfluxDBController(InfluxDbService influxDbService)
        {
            _influxDbService = influxDbService;
        }

        [HttpGet("connect")]
        public IActionResult Connect()
        {
            _influxDbService.Connect();
            return Ok("Connected to InfluxDB");
        }

        [HttpPost("write")]
        public async Task<IActionResult> WriteData(
            string animalId,
            double latitude,
            double longitude,
            double altitude)
        {
            await _influxDbService.WriteDataAsync(animalId, latitude, longitude, altitude);
            return Ok("Data written successfully");
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryData(string org, string fluxQuery)
        {
            var results = await _influxDbService.QueryDataAsync(org, fluxQuery);
            return Ok(results);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteData(string bucket, string org, string predicate)
        {
            await _influxDbService.DeleteDataAsync(bucket, org, predicate);
            return Ok("Data deleted successfully");
        }

        [HttpGet("{animalId}")]
        public async Task<IActionResult> GetAnimalLocation(string animalId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(animalId))
                {
                    return BadRequest("Animal ID cannot be empty");
                }

                var results = await _influxDbService.GetAnimalPositionsLastHourAsync(animalId);

                // Check if any data was found
                if (results == null || !results.Any())
                {
                    return NotFound($"No location data found for animal {animalId} in the last hour");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use ILogger here)
                // _logger.LogError(ex, "Error retrieving animal location for {AnimalId}", animalId);

                return StatusCode(500, "An error occurred while retrieving animal location data");
            }
        }
    }
}