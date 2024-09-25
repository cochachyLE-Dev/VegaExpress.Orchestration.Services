namespace VegaExpress.Agent.Data.Models
{
    public class MetricsModel
    {
        public double Traffic { get; set; }
        public double ErrorRate { get; set; }
        public double ResponseTime { get; set; }
        public double Throughput { get; set; }
        public double availability { get; set; }
        public double latency { get; set; }
    }
}