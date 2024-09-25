namespace VegaExpress.Agent.Data.Models
{
    public class NetworkAddressModel
    {
        public string? Name { get; set; }
        public string? MAC { get; set; }
        public string? IPv4 { get; set; }
        public string? IPv6 { get; set; }
        public long Speed { get; set; }
    }
}