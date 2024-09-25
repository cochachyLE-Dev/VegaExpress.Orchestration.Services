namespace VegaExpress.Agent.Data.Models
{
    public class GatewayRoutingEndPointModel
    {
        public int ID { get; set; }
        public string? Method { get; set; }
        public string? Pattern { get; set; }
        public string? Location { get; set; }
        public string? ServiceUID { get; set; }
    }
}
