using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Interfaces;

namespace VegaExpress.Agent.Data.Models
{
    public class GatewayRoutingServiceModel: IServiceInstance
    {
        public string? ServiceUID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceLocation { get; set; }
        public string? ServiceVersion { get; set; }
        public string? ServiceAddress { get; set; }
        public string? ServiceAgentUID { get; set; }
        public ServiceState ServiceState { get; set; }
        public bool ServiceIsBusy { get; set; }
        public DateTime LatestSession { get; set; }
        public int ProcessID { get; set; }
        public string? ProcessName { get; set; }
        public ServerModel? Server { get; set; }

        public string? ServiceStartType { get; set; }
        public string? Color { get; set; }

        public IEnumerable<NetworkAddressModel>? networkAddresses { get; set; }
        public IEnumerable<EndpointRoute>? EndpointRoutes { get; set; }
    }
    public class EndpointRoute
    {        
        public int ID { get; set; }
        public HttpMethod? Method { get; set; }
        public string? Pattern { get; set; }
        public string? Location { get; set; }
        public bool RequireAuthentication { get; set; }
    }
    public enum HttpMethod
    { 
        GET,
        POST,
        PUT,            
        DELETE,
        PATCH
    }
    public class LoadBalancer
    {
        public string? Name { get; set; }
        public string? ProxyServiceUID { get; set; }
        public IEnumerable<WorkerServiceInstanceModel>? Instances { get; set; }
    }
}