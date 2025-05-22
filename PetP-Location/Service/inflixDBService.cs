using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using PetP_Location.Model;

namespace PetP_Location.Service
{
    public class InfluxDbService
    {
        private readonly InfluxDbSettings _settings;
        private InfluxDBClient _client;

        public InfluxDbService(IOptions<InfluxDbSettings> options)
        {
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public void Connect()
        {
            if (_client == null)
            {
                _client = InfluxDBClientFactory.Create($"http://{_settings.IP}", _settings.Token);
            }
        }

        public async Task WriteDataAsync(
            string animalId,
            double latitude,
            double longitude,
            double altitude)
        {
            if (_client == null)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var writeApi = _client.GetWriteApiAsync();
            var point = PointData.Measurement("Animal_posistion")
                .Tag("Animal", animalId)
                .Field("latitude", latitude)
                .Field("longitude", longitude)
                .Field("altitude", altitude)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(point, "Location", "PetP");
        }

        public async Task<List<Dictionary<string, object>>> QueryDataAsync(string org, string fluxQuery)
        {
            if (_client == null) throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(fluxQuery, org);

            var results = new List<Dictionary<string, object>>();
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var result = new Dictionary<string, object>
                    {
                        { "Time", record.GetTime()?.ToDateTimeUtc() },
                        { "Value", record.GetValue() }
                    };
                    results.Add(result);
                }
            }
            return results;
        }

        public async Task DeleteDataAsync(string bucket, string org, string predicate)
        {
            if (_client == null) throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var deleteApi = _client.GetDeleteApi();
            await deleteApi.Delete(DateTime.MinValue, DateTime.MaxValue, predicate, bucket, org);
        }

        public async Task<List<AnimalPosition>> GetAnimalPositionsLastHourAsync(string animalId)
        {
            if (_client == null)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var queryApi = _client.GetQueryApi();

            var flux = $@"
            from(bucket: ""Location"")
            |> range(start: -1h)
            |> filter(fn: (r) => r._measurement == ""Animal_posistion"")
            |> filter(fn: (r) => r.Animal == ""{animalId}"")
            |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")";

            var tables = await queryApi.QueryAsync(flux, "PetP");
            var positions = new List<AnimalPosition>();

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    positions.Add(new AnimalPosition
                    {
                        AnimalId = record.GetValueByKey("Animal")?.ToString(),
                        Latitude = Convert.ToDouble(record.GetValueByKey("latitude")),
                        Longitude = Convert.ToDouble(record.GetValueByKey("longitude")),
                        Altitude = Convert.ToDouble(record.GetValueByKey("altitude")),
                        Timestamp = record.GetTime()?.ToDateTimeUtc() ?? default(DateTime)
                    });
                }
            }

            return positions.OrderBy(p => p.Timestamp).ToList();
        }
    }
}
