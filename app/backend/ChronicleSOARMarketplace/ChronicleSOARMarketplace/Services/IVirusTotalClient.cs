using System.Threading.Tasks;
using ChronicleSOARMarketplace.Models;

namespace ChronicleSOARMarketplace.Services
{
    public interface IVirusTotalClient
    {
        Task<IoC> AnalyzeIoCAsync(string ioc);
    }
}
