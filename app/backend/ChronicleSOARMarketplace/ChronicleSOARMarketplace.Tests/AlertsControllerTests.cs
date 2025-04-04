using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace ChronicleSOARMarketplace.Tests
{
    public class AlertsControllerTests
    {
        private readonly string _outputDir = "TestOutput";

        [Fact]
        public async Task GetAlerts_ReturnsEnrichedAlerts()
        {
            // ✅ Conditionally skip test
            if (!Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS")?.Equals("true") ?? true)
            {
                return;
            }

            // ✅ Move WebApplicationFactory inside the test method
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "Output:Directory", _outputDir }
                    });
                });
            });

            using var client = factory.CreateClient();

            // Ensure output directory exists
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);

            // Arrange: Write test alert
            var testAlert = new Alert
            {
                AlertId = "test-alert",
                Severity = 100,
                IoCs = new System.Collections.Generic.List<IoC>
                {
                    new IoC
                    {
                        Identifier = "1.2.3.4",
                        IsMalicious = true,
                        Country = "US",
                        LastModificationDate = "2025-04-01T00:00:00Z",
                        LastAnalysisStats = new LastAnalysisStats
                        {
                            Harmless = 0,
                            Malicious = 5,
                            Suspicious = 0,
                            Timeout = 0,
                            Undetected = 0
                        },
                        Tags = new System.Collections.Generic.List<string> { "malware" },
                        ReportLink = "http://example.com/1.2.3.4"
                    }
                }
            };

            var filePath = Path.Combine(_outputDir, "test-alert.json");
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(testAlert));

            // Act
            var response = await client.GetAsync("/api/alerts");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var alerts = JsonSerializer.Deserialize<System.Collections.Generic.List<Alert>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(alerts);
            Assert.NotEmpty(alerts);
            Assert.Contains(alerts, a => a.AlertId == "test-alert");

            // Cleanup
            File.Delete(filePath);
        }
    }
}
