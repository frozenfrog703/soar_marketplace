using System;
using System.Linq;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using Microsoft.Extensions.Logging;

namespace ChronicleSOARMarketplace.Services
{
    public class EnrichmentService
    {
        private readonly IVirusTotalClient _virusTotalClient;
        private readonly ILogger<EnrichmentService> _logger;

        public EnrichmentService(IVirusTotalClient virusTotalClient, ILogger<EnrichmentService> logger)
        {
            _virusTotalClient = virusTotalClient;
            _logger = logger;
        }

        public virtual async Task<Alert> EnrichAlertAsync(Alert alert)
        {
            int maliciousCount = 0;
            for (int i = 0; i < alert.IoCs.Count; i++)
            {
                try
                {
                    var result = await _virusTotalClient.AnalyzeIoCAsync(alert.IoCs[i].Identifier);
                    alert.IoCs[i] = result;
                    if (result.IsMalicious)
                        maliciousCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error analyzing IoC {alert.IoCs[i].Identifier}");
                }
            }

            if (alert.IoCs.Any())
                alert.Severity = (int)Math.Round((double)(maliciousCount * 100) / alert.IoCs.Count);
            else
                alert.Severity = 0;

            return alert;
        }
    }
}
