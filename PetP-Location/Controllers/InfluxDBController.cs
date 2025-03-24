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
        public async Task<IActionResult> WriteData(string bucket,string measurement,string fieldName, int fieldNumber)
        {
            await _influxDbService.WriteDataAsync(bucket, measurement, fieldName,fieldNumber);
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
    }
}