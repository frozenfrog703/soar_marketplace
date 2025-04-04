using System.Collections.Generic;

namespace ChronicleSOARMarketplace.Models
{
    public class IoC
    {
        public string Identifier { get; set; }
        public bool IsMalicious { get; set; }
        public string Country { get; set; } = "Unknown";
        public string LastModificationDate { get; set; }
        public LastAnalysisStats LastAnalysisStats { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string ReportLink { get; set; }
    }
}
