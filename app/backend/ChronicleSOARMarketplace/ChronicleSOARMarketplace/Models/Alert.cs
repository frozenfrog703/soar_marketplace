using System;
using System.Collections.Generic;

namespace ChronicleSOARMarketplace.Models
{
    public class Alert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public int Severity { get; set; }
        public List<IoC> IoCs { get; set; } = new();
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
