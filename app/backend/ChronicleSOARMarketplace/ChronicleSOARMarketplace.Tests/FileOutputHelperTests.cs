using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using ChronicleSOARMarketplace.Utils;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ChronicleSOARMarketplace.Tests
{
    public class FileOutputHelperTests
    {
        [Fact]
        public async Task WriteAlertToFileAsync_WritesValidJsonFile()
        {
            // Arrange: Create a test alert.
            var testAlert = new Alert
            {
                AlertId = "test-alert-123",
                Severity = 80,
                IoCs = new System.Collections.Generic.List<IoC>
                {
                    new IoC
                    {
                        Identifier = "9.9.9.9",
                        IsMalicious = true,
                        Country = "US",
                        LastModificationDate = DateTime.UtcNow.ToString("o"),
                        LastAnalysisStats = new LastAnalysisStats { Harmless = 0, Malicious = 3, Suspicious = 0, Timeout = 0, Undetected = 0 },
                        Tags = new System.Collections.Generic.List<string> { "malware" },
                        ReportLink = "http://example.com/9.9.9.9"
                    }
                }
            };

            // Use in-memory configuration to set the output directory.
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> {
                {"Output:Directory", "TestFileOutput"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act: Write the alert to file.
            await FileOutputHelper.WriteAlertToFileAsync(testAlert, configuration);

            // Assert: Verify that a file was created and contains valid JSON.
            string[] files = Directory.GetFiles("TestFileOutput", "*.json");
            Assert.NotEmpty(files);

            string json = await File.ReadAllTextAsync(files[0]);
            var deserializedAlert = JsonSerializer.Deserialize<Alert>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal("test-alert-123", deserializedAlert.AlertId);

            // Cleanup: Delete the created file(s) and directory.
            foreach (var file in files)
            {
                File.Delete(file);
            }
            Directory.Delete("TestFileOutput");
        }
    }
}
