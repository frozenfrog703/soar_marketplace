using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;
using Microsoft.Extensions.Configuration;

namespace ChronicleSOARMarketplace.Services
{
    public class VirusTotalClient : IVirusTotalClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public VirusTotalClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("VirusTotalClient");
            _apiKey = configuration["VirusTotal:ApiKey"];
        }

        public async Task<IoC> AnalyzeIoCAsync(string ioc)
        {
            // Construct the request URL. In a real implementation, consult the VirusTotal v3 API docs.
            var requestUrl = $"https://www.virustotal.com/api/v3/ip_addresses/{ioc}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("x-apikey", _apiKey);

            // For the purpose of demonstration, need to simulate a call.
            // Uncomment the following lines to make a real call.
            // var response = await _httpClient.SendAsync(request);
            // response.EnsureSuccessStatusCode();
            // var json = await response.Content.ReadAsStringAsync();
            // var result = JsonSerializer.Deserialize<VirusTotalResponse>(json);

            // Simulated result:
            var random = new Random();
            bool isMalicious = random.Next(0, 2) == 1;
            var iocResult = new IoC
            {
                Identifier = ioc,
                IsMalicious = isMalicious,
                Country = "US",
                LastModificationDate = DateTime.UtcNow.ToString("o"),
                LastAnalysisStats = new LastAnalysisStats
                {
                    Harmless = random.Next(0, 10),
                    Malicious = isMalicious ? random.Next(1, 5) : 0,
                    Suspicious = random.Next(0, 3),
                    Timeout = 0,
                    Undetected = random.Next(0, 10)
                },
                Tags = isMalicious ? new() { "suspicious", "malware" } : new() { "clean" },
                ReportLink = $"https://www.virustotal.com/gui/ip-address/{ioc}/detection"
            };

            // In a real implementation, need to process result data.
            return await Task.FromResult(iocResult);
        }
    }
}
