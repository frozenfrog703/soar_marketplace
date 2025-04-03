using System.Text.Json;
using ChronicleSOARMarketplace.Models;

namespace ChronicleSOARMarketplace.Utils
{
    public class FileOutputHelper
    {
        public static async Task WriteAlertToFileAsync(Alert alert, IConfiguration configuration)
        {
            var outputDir = configuration["Output:Directory"];
            Directory.CreateDirectory(outputDir);
            string fileName = Path.Combine(outputDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(alert, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
