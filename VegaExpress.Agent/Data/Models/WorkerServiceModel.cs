using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Data.Models
{
    public class WorkerServiceModel
    {
        public string? ServiceUID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceVersion { get; set; }
        public string? ServiceAddress { get; set; }
        public string? ServicePortRange { get; set; }
        public string? GatewayServiceAddress { get; set; }
        public int ServiceTrafficLimit { get; set; }
        public int ServiceErrorRateLimit { get; set; }
        public int ServiceInstanceLimit { get; set; }
        public string? ServiceLocation { get; set; }
        public string? ServiceStartType { get; set; }        
        public string? Color { get; set; }          
    }            
}