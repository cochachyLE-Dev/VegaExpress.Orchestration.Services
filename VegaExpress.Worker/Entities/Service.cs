namespace VegaExpress.Worker.Entities
{
    public class ServiceRegistrationEx
    {
        public string? Name { get; set; }    
        public string? Location { get; set; }
        public string? Version { get; set; }
        public string? Metadata { get; set; }
        public HealthCheckEx? Monitoring { get; set; }
        public LoadBalancingEx? LoadBalancer { get; set; }
    }
}
