using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using ChronicleSOARMarketplace.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace ChronicleSOARMarketplace.Tests
{
    public class EnrichmentServiceTests
    {
        [Fact]
        public async Task EnrichAlertAsync_ComputesSeverityCorrectly()
        {
            // Arrange: Create a mock VirusTotal client with predetermined responses.
            var mockVirusTotalClient = new Mock<IVirusTotalClient>();
            var logger = new LoggerFactory().CreateLogger<EnrichmentService>();

            // Setup the mock to simulate one malicious IoC and one clean IoC.
            mockVirusTotalClient.Setup(v => v.AnalyzeIoCAsync("1.1.1.1")).ReturnsAsync(new IoC
            {
                Identifier = "1.1.1.1",
                IsMalicious = true,
                Country = "US",
                LastModificationDate = "2025-04-01T00:00:00Z",
                LastAnalysisStats = new LastAnalysisStats { Harmless = 0, Malicious = 3, Suspicious = 0, Timeout = 0, Undetected = 0 },
                Tags = new System.Collections.Generic.List<string> { "malware" },
                ReportLink = "http://example.com/1.1.1.1"
            });

            mockVirusTotalClient.Setup(v => v.AnalyzeIoCAsync("8.8.8.8")).ReturnsAsync(new IoC
            {
                Identifier = "8.8.8.8",
                IsMalicious = false,
                Country = "US",
                LastModificationDate = "2025-04-01T00:00:00Z",
                LastAnalysisStats = new LastAnalysisStats { Harmless = 5, Malicious = 0, Suspicious = 0, Timeout = 0, Undetected = 5 },
                Tags = new System.Collections.Generic.List<string> { "clean" },
                ReportLink = "http://example.com/8.8.8.8"
            });

            var enrichmentService = new EnrichmentService(mockVirusTotalClient.Object, logger);
            var alert = new Alert();
            alert.IoCs.Add(new IoC { Identifier = "1.1.1.1" });
            alert.IoCs.Add(new IoC { Identifier = "8.8.8.8" });

            // Act: Enrich the alert.
            var enrichedAlert = await enrichmentService.EnrichAlertAsync(alert);

            // Assert: Verify severity (1 malicious out of 2 should be 50%) and the IoC results.
            Assert.Equal(50, enrichedAlert.Severity);
            Assert.Collection(enrichedAlert.IoCs,
                ioc => Assert.True(ioc.IsMalicious),
                ioc => Assert.False(ioc.IsMalicious)
            );
        }

        [Fact]
        public async Task EnrichAlertAsync_NoIoCs_ShouldHaveZeroSeverity()
        {
            // Arrange
            var mockVirusTotalClient = new Mock<IVirusTotalClient>();
            var logger = new LoggerFactory().CreateLogger<EnrichmentService>();

            var enrichmentService = new EnrichmentService(mockVirusTotalClient.Object, logger);
            var alert = new Alert(); // No IoCs added

            // Act
            var enrichedAlert = await enrichmentService.EnrichAlertAsync(alert);

            // Assert: With no IoCs, severity should be zero.
            Assert.Equal(0, enrichedAlert.Severity);
            Assert.Empty(enrichedAlert.IoCs);
        }
    }
}
