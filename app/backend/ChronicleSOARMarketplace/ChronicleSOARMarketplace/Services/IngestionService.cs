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
using Grpc.Core;

namespace ChronicleSOARMarketplace.Services
{
    public class IngestionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider; // Use IServiceProvider for Scoped Services
        private readonly ILogger<IngestionService> _logger;
        private readonly SubscriberServiceApiClient _subscriberClient;
        private readonly SubscriptionName _subscriptionName;
        private readonly string _outputDirectory;
        private const int TimerIntervalSeconds = 300; // 5 minutes

        public IngestionService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<IngestionService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            string subscriptionName = _configuration["PubSub:SubscriptionName"];
            _subscriptionName = SubscriptionName.Parse(subscriptionName);
            _outputDirectory = _configuration["Output:Directory"];
            Directory.CreateDirectory(_outputDirectory);

            // Load service account key explicitly
            var credentialPath = _configuration["PubSub:ServiceAccountKeyPath"];
            GoogleCredential credential = GoogleCredential.FromFile(credentialPath);

            // Create Pub/Sub Subscriber Client with credentials
            var clientBuilder = new SubscriberServiceApiClientBuilder
            {
                Credential = credential
            };
            _subscriberClient = clientBuilder.Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ingestion Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Pull messages from Pub/Sub subscription.
                    var response = _subscriberClient.Pull(new PullRequest
                    {
                        Subscription = _subscriptionName.ToString(),
                        ReturnImmediately = false,
                        MaxMessages = 10
                    });

                    if (response.ReceivedMessages.Count > 0)
                    {
                        foreach (var msg in response.ReceivedMessages)
                        {
                            string messageData = System.Text.Encoding.UTF8.GetString(msg.Message.Data.ToByteArray());
                            _logger.LogInformation($"Message received: {messageData}");

                            // Process message using a Scoped service
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var enrichmentService = scope.ServiceProvider.GetRequiredService<EnrichmentService>();

                                var alert = new Alert();
                                var iocLines = messageData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                foreach (var line in iocLines)
                                {
                                    alert.IoCs.Add(new IoC { Identifier = line.Trim() });
                                }

                                // Enrich the Alert.
                                var enrichedAlert = await enrichmentService.EnrichAlertAsync(alert);

                                // Write output JSON file.
                                string fileName = Path.Combine(_outputDirectory, $"{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                                await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(enrichedAlert, new JsonSerializerOptions { WriteIndented = true }));

                                // Acknowledge the message.
                                _subscriberClient.Acknowledge(new AcknowledgeRequest
                                {
                                    Subscription = _subscriptionName.ToString(),
                                    AckIds = { msg.AckId }
                                });
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No messages found.");
                    }
                }
                catch (RpcException rpcEx)
                {
                    _logger.LogError(rpcEx, "Pub/Sub RPC Error: {Message}", rpcEx.Message);
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
