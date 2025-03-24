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

        public async Task WriteDataAsync(string bucket, string measurement, string fieldName, int fieldNumber) 
        {
            if (_client == null) throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var writeApi = _client.GetWriteApiAsync();
            var point = PointData.Measurement(measurement)
                .Field(fieldName,fieldNumber)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(point, bucket, "PetP");
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
    }
}
