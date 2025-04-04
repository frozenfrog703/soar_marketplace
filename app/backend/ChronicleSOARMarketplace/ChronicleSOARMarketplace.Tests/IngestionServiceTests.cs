using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using ChronicleSOARMarketplace.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChronicleSOARMarketplace.Tests
{
    public class IngestionServiceTests
    {
        private readonly string _outputDir = Path.Combine(Directory.GetCurrentDirectory(), "test-output");

        public IngestionServiceTests()
        {
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);
        }

        [Fact]
        public async Task ProcessTestMessageAsync_CreatesOutputFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IngestionService>>();
            var mockEnrichmentService = new Mock<EnrichmentService>(MockBehavior.Strict, null, null);
            mockEnrichmentService
                .Setup(es => es.EnrichAlertAsync(It.IsAny<Alert>()))
                .ReturnsAsync((Alert alert) => alert);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(EnrichmentService)))
                .Returns(mockEnrichmentService.Object);

            if (!Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS")?.Equals("true") ?? true)
            {
                return;
            }

            var inMemorySettings = new Dictionary<string, string>
            {
                {"Output:Directory", _outputDir},
                {"PubSub:SubscriptionName", "projects/test-project/subscriptions/test-sub"},
                {"PubSub:ProjectId", "test-project"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new TestableIngestionService(configuration, mockServiceProvider.Object, mockLogger.Object);

            var alert = new Alert
            {
                AlertId = Guid.NewGuid().ToString(),
                Severity = 10,
                IoCs = new List<IoC>
                {
                    new IoC
                    {
                        Identifier = "1.2.3.4",
                        IsMalicious = true,
                        Country = "US",
                        LastModificationDate = "2025-04-01T00:00:00Z",
                        LastAnalysisStats = new LastAnalysisStats { Harmless = 0, Malicious = 5 },
                        Tags = new List<string> { "malware" },
                        ReportLink = "http://example.com/1.2.3.4"
                    }
                }
            };

            var message = JsonSerializer.Serialize(alert);

            // Act
            await service.ProcessTestMessageAsync(message);

            // Assert
            var files = Directory.GetFiles(_outputDir, "*.json");
            Assert.True(files.Length > 0);

            // Cleanup
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }

    public class TestableIngestionService : IngestionService
    {
        private readonly EnrichmentService _enrichmentService;
        private readonly IConfiguration _configuration;

        public TestableIngestionService(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<IngestionService> logger
        ) : base(configuration, serviceProvider, logger)
        {
            _configuration = configuration;
            _enrichmentService = (EnrichmentService)serviceProvider.GetService(typeof(EnrichmentService));
        }

        public async Task ProcessTestMessageAsync(string message)
        {
            var alert = JsonSerializer.Deserialize<Alert>(message);
            if (alert != null)
            {
                await _enrichmentService.EnrichAlertAsync(alert);

                var outputDirectory = _configuration["Output:Directory"];
                var outputFilePath = Path.Combine(outputDirectory, $"alert_{alert.AlertId}.json");

                var enrichedJson = JsonSerializer.Serialize(alert, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(outputFilePath, enrichedJson);
            }
        }
    }
}
