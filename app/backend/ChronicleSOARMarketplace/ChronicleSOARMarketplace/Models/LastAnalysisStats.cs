namespace ChronicleSOARMarketplace.Models
{
    public class LastAnalysisStats
    {
        public int Harmless { get; set; }
        public int Malicious { get; set; }
        public int Suspicious { get; set; }
        public int Timeout { get; set; }
        public int Undetected { get; set; }
    }
}
