namespace VegaExpress.Worker.Entities
{
    public class LoadBalancingEx
    {        
        public int NumberOfRequests { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string? ServiceLocation { get; set; }
    }
}
