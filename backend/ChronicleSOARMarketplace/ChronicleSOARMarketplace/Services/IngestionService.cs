using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Google.Cloud.PubSub.V1;
using Google.Apis.Auth.OAuth2;

namespace ChronicleSOARMarketplace.Services
{
    public class IngestionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EnrichmentService _enrichmentService;
        private readonly ILogger<IngestionService> _logger;
        private readonly SubscriberServiceApiClient _subscriberClient;
        private readonly string _subscriptionName;
        private readonly string _outputDirectory;
        private const int TimerIntervalSeconds = 300; // 5 minutes

        public IngestionService(IConfiguration configuration, EnrichmentService enrichmentService, ILogger<IngestionService> logger)
        {
            _configuration = configuration;
            _enrichmentService = enrichmentService;
            _logger = logger;

            _subscriptionName = _configuration["PubSub:SubscriptionName"];
            _outputDirectory = _configuration["Output:Directory"];
            Directory.CreateDirectory(_outputDirectory);

            // Authenticate using the service account key.
            var credentialPath = _configuration["PubSub:ServiceAccountKeyPath"];
            GoogleCredential credential = GoogleCredential.FromFile(credentialPath);
            _subscriberClient = SubscriberServiceApiClient.Create(credential);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ingestion Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Pull messages from Pub/Sub subscription.
                    var response = _subscriberClient.Pull(_subscriptionName, returnImmediately: true, maxMessages: 10);
                    if (response.ReceivedMessages.Count > 0)
                    {
                        foreach (var msg in response.ReceivedMessages)
                        {
                            string messageData = Encoding.UTF8.GetString(msg.Message.Data.ToByteArray());
                            _logger.LogInformation($"Message received: {messageData}");

                            // Create Alert object from the message.
                            var alert = new Alert();
                            var iocLines = messageData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in iocLines)
                            {
                                alert.IoCs.Add(new IoC { Identifier = line.Trim() });
                            }

                            // Enrich the Alert.
                            var enrichedAlert = await _enrichmentService.EnrichAlertAsync(alert);

                            // Write output JSON file.
                            string fileName = Path.Combine(_outputDirectory, $"{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                            await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(enrichedAlert, new JsonSerializerOptions { WriteIndented = true }));

                            // Acknowledge the message.
                            _subscriberClient.Acknowledge(_subscriptionName, new[] { msg.AckId });
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No messages found.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during ingestion.");
                }

                await Task.Delay(TimeSpan.FromSeconds(TimerIntervalSeconds), stoppingToken);
            }
        }
    }
}
