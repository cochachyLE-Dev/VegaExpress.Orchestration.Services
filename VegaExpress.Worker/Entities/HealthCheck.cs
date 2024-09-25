namespace VegaExpress.Worker.Entities
{
    public class HealthCheckEx
    {
        public bool Availability { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public Dictionary<string, double>? ResourceUsage { get; set; } // This could be a dictionary with 'cpu' and 'memory' as keys
        public double ErrorRate { get; set; }
        public int Traffic { get; set; } // Number of requests the service handles
    }
}
