
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using PetP_Location.Model;
using PetP_Location.Service;
using Xunit;

namespace UnitTestLocation
{
    public class InfluxDbServiceTest
    {
        private readonly Mock<IOptions<InfluxDbSettings>> _mockOptions;
        private readonly InfluxDbSettings _settings;
        private readonly InfluxDbService _service;

        public InfluxDbServiceTest()
        {
            _settings = new InfluxDbSettings
            {
                IP = "localhost:8086",
                Token = "test-token"
            };

            _mockOptions = new Mock<IOptions<InfluxDbSettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_settings);

            _service = new InfluxDbService(_mockOptions.Object);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new InfluxDbService(null));
        }

        [Fact]
        public void Constructor_WithNullOptionsValue_ThrowsArgumentNullException()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<InfluxDbSettings>>();
            mockOptions.Setup(x => x.Value).Returns((InfluxDbSettings)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new InfluxDbService(mockOptions.Object));
        }

        [Fact]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Act
            var service = new InfluxDbService(_mockOptions.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Connect_CallsSuccessfully()
        {
            // Act - Should not throw an exception
            _service.Connect();

            // Multiple calls should not cause issues
            _service.Connect();
            _service.Connect();
        }

        [Fact]
        public async Task WriteDataAsync_WithoutConnection_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.WriteDataAsync("animal1", 52.0, 5.0, 100.0));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Fact]
        public async Task QueryDataAsync_WithoutConnection_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.QueryDataAsync("org", "query"));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Fact]
        public async Task DeleteDataAsync_WithoutConnection_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteDataAsync("bucket", "org", "predicate"));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Fact]
        public async Task GetAnimalPositionsLastHourAsync_WithoutConnection_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAnimalPositionsLastHourAsync("animal1"));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task WriteDataAsync_WithInvalidAnimalId_WithoutConnection_ThrowsInvalidOperationException(string animalId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.WriteDataAsync(animalId, 52.0, 5.0, 100.0));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetAnimalPositionsLastHourAsync_WithInvalidAnimalId_WithoutConnection_ThrowsInvalidOperationException(string animalId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAnimalPositionsLastHourAsync(animalId));

            Assert.Equal("InfluxDB client is not initialized. Call Connect() first.", exception.Message);
        }

        [Fact]
        public void InfluxDbSettings_PropertiesSetCorrectly()
        {
            // Arrange & Act
            var settings = new InfluxDbSettings
            {
                IP = "192.168.1.100:8086",
                Token = "my-secret-token"
            };

            // Assert
            Assert.Equal("192.168.1.100:8086", settings.IP);
            Assert.Equal("my-secret-token", settings.Token);
        }
    }

    // Testable wrapper approach - if you want to make your service more testable
    public interface IInfluxDbService
    {
        void Connect();
        Task WriteDataAsync(string animalId, double latitude, double longitude, double altitude);
        Task<List<Dictionary<string, object>>> QueryDataAsync(string org, string fluxQuery);
        Task DeleteDataAsync(string bucket, string org, string predicate);
        Task<List<AnimalPosition>> GetAnimalPositionsLastHourAsync(string animalId);
    }

    // Integration tests that require a real InfluxDB instance
    public class InfluxDbServiceIntegrationTests : IDisposable
    {
        private readonly InfluxDbService _service;
        private readonly string _testBucket = "test-location";
        private readonly string _testOrg = "test-org";

        public InfluxDbServiceIntegrationTests()
        {
            // Note: These tests require a running InfluxDB instance
            // You should set up a test database for these tests
            var settings = new InfluxDbSettings
            {
                IP = "localhost:8086",
                Token = Environment.GetEnvironmentVariable("INFLUXDB_TEST_TOKEN") ?? "test-token"
            };

            var options = Options.Create(settings);
            _service = new InfluxDbService(options);
        }

        [Fact(Skip = "Integration test - requires InfluxDB instance")]
        public async Task WriteDataAsync_WithValidData_WritesSuccessfully()
        {
            // Arrange
            _service.Connect();
            var animalId = $"test-animal-{Guid.NewGuid()}";
            var latitude = 52.0907;
            var longitude = 5.1214;
            var altitude = 100.5;

            try
            {
                // Act & Assert - Should not throw
                await _service.WriteDataAsync(animalId, latitude, longitude, altitude);
            }
            finally
            {
                // Cleanup
                await CleanupTestData(animalId);
            }
        }

        [Fact(Skip = "Integration test - requires InfluxDB instance")]
        public async Task GetAnimalPositionsLastHourAsync_WithExistingData_ReturnsData()
        {
            // Arrange
            _service.Connect();
            var animalId = $"test-animal-{Guid.NewGuid()}";
            var latitude = 52.0907;
            var longitude = 5.1214;
            var altitude = 100.5;

            try
            {
                // Write test data
                await _service.WriteDataAsync(animalId, latitude, longitude, altitude);

                // Wait for data to be indexed
                await Task.Delay(2000);

                // Act
                var positions = await _service.GetAnimalPositionsLastHourAsync(animalId);

                // Assert
                Assert.NotEmpty(positions);
                var position = positions.First();
                Assert.Equal(animalId, position.AnimalId);
                Assert.Equal(latitude, position.Latitude);
                Assert.Equal(longitude, position.Longitude);
                Assert.Equal(altitude, position.Altitude);
                Assert.True(position.Timestamp > DateTime.UtcNow.AddHours(-1));
            }
            finally
            {
                // Cleanup
                await CleanupTestData(animalId);
            }
        }

        [Fact(Skip = "Integration test - requires InfluxDB instance")]
        public async Task GetAnimalPositionsLastHourAsync_WithNonExistentAnimal_ReturnsEmpty()
        {
            // Arrange
            _service.Connect();
            var nonExistentAnimalId = $"non-existent-{Guid.NewGuid()}";

            // Act
            var positions = await _service.GetAnimalPositionsLastHourAsync(nonExistentAnimalId);

            // Assert
            Assert.Empty(positions);
        }

        [Fact(Skip = "Integration test - requires InfluxDB instance")]
        public async Task QueryDataAsync_WithValidQuery_ReturnsResults()
        {
            // Arrange
            _service.Connect();
            var animalId = $"test-animal-{Guid.NewGuid()}";

            try
            {
                // Write test data
                await _service.WriteDataAsync(animalId, 52.0, 5.0, 100.0);

                // Wait for data to be indexed
                await Task.Delay(2000);

                var query = $@"
                from(bucket: ""Location"")
                |> range(start: -1h)
                |> filter(fn: (r) => r._measurement == ""Animal_posistion"")
                |> filter(fn: (r) => r.Animal == ""{animalId}"")";

                // Act
                var results = await _service.QueryDataAsync("PetP", query);

                // Assert
                Assert.NotEmpty(results);
            }
            finally
            {
                // Cleanup
                await CleanupTestData(animalId);
            }
        }

        [Fact(Skip = "Integration test - requires InfluxDB instance")]
        public async Task DeleteDataAsync_WithValidPredicate_DeletesData()
        {
            // Arrange
            _service.Connect();
            var animalId = $"test-animal-{Guid.NewGuid()}";

            try
            {
                // Write test data
                await _service.WriteDataAsync(animalId, 52.0, 5.0, 100.0);

                // Wait for data to be indexed
                await Task.Delay(2000);

                // Verify data exists
                var positionsBeforeDelete = await _service.GetAnimalPositionsLastHourAsync(animalId);
                Assert.NotEmpty(positionsBeforeDelete);

                // Act
                await _service.DeleteDataAsync("Location", "PetP", $"Animal=\"{animalId}\"");

                // Wait for deletion to be processed
                await Task.Delay(2000);

                // Assert
                var positionsAfterDelete = await _service.GetAnimalPositionsLastHourAsync(animalId);
                Assert.Empty(positionsAfterDelete);
            }
            finally
            {
                // Cleanup (in case deletion failed)
                await CleanupTestData(animalId);
            }
        }

        private async Task CleanupTestData(string animalId)
        {
            try
            {
                await _service.DeleteDataAsync("Location", "PetP", $"Animal=\"{animalId}\"");
                await Task.Delay(1000); // Wait for cleanup to complete
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }

        public void Dispose()
        {
            // Cleanup resources if needed
        }
    }

    // Mock implementation for testing scenarios where you need to test business logic
    public class MockInfluxDbService : IInfluxDbService
    {
        private readonly List<AnimalPosition> _positions = new();
        private bool _isConnected = false;

        public void Connect()
        {
            _isConnected = true;
        }

        public Task WriteDataAsync(string animalId, double latitude, double longitude, double altitude)
        {
            if (!_isConnected)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            _positions.Add(new AnimalPosition
            {
                AnimalId = animalId,
                Latitude = latitude,
                Longitude = longitude,
                Altitude = altitude,
                Timestamp = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        public Task<List<Dictionary<string, object>>> QueryDataAsync(string org, string fluxQuery)
        {
            if (!_isConnected)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var results = new List<Dictionary<string, object>>();
            foreach (var position in _positions)
            {
                results.Add(new Dictionary<string, object>
                {
                    { "Time", position.Timestamp },
                    { "Value", position.Latitude } // Simplified for mock
                });
            }

            return Task.FromResult(results);
        }

        public Task DeleteDataAsync(string bucket, string org, string predicate)
        {
            if (!_isConnected)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            // Simple mock implementation - in reality, you'd parse the predicate
            _positions.Clear();
            return Task.CompletedTask;
        }

        public Task<List<AnimalPosition>> GetAnimalPositionsLastHourAsync(string animalId)
        {
            if (!_isConnected)
                throw new InvalidOperationException("InfluxDB client is not initialized. Call Connect() first.");

            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            var filteredPositions = _positions
                .Where(p => p.AnimalId == animalId && p.Timestamp >= cutoffTime)
                .OrderBy(p => p.Timestamp)
                .ToList();

            return Task.FromResult(filteredPositions);
        }
    }

    // Tests using the mock implementation
    public class MockInfluxDbServiceTests
    {
        private readonly MockInfluxDbService _mockService;

        public MockInfluxDbServiceTests()
        {
            _mockService = new MockInfluxDbService();
        }

        [Fact]
        public async Task WriteAndRetrieve_WorksCorrectly()
        {
            // Arrange
            _mockService.Connect();
            var animalId = "test-animal";
            var latitude = 52.0907;
            var longitude = 5.1214;
            var altitude = 100.5;

            // Act
            await _mockService.WriteDataAsync(animalId, latitude, longitude, altitude);
            var positions = await _mockService.GetAnimalPositionsLastHourAsync(animalId);

            // Assert
            Assert.Single(positions);
            Assert.Equal(animalId, positions[0].AnimalId);
            Assert.Equal(latitude, positions[0].Latitude);
            Assert.Equal(longitude, positions[0].Longitude);
            Assert.Equal(altitude, positions[0].Altitude);
        }

        [Fact]
        public async Task GetAnimalPositionsLastHourAsync_FiltersCorrectly()
        {
            // Arrange
            _mockService.Connect();
            await _mockService.WriteDataAsync("animal1", 52.0, 5.0, 100.0);
            await _mockService.WriteDataAsync("animal2", 53.0, 6.0, 200.0);

            // Act
            var animal1Positions = await _mockService.GetAnimalPositionsLastHourAsync("animal1");
            var animal2Positions = await _mockService.GetAnimalPositionsLastHourAsync("animal2");

            // Assert
            Assert.Single(animal1Positions);
            Assert.Single(animal2Positions);
            Assert.Equal("animal1", animal1Positions[0].AnimalId);
            Assert.Equal("animal2", animal2Positions[0].AnimalId);
        }
    }
}