using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Data.Models
{
    public class ClientServiceModel
    {
        public string? ServiceUID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceVersion { get; set; }
        public string? ServiceAddress { get; set; }
        public string? ServiceAgentUID { get; set; }
        public ServiceState ServiceState { get; set; }
        public bool ServiceIsBusy { get; set; }
        public DateTime LatestSession { get; set; } = DateTime.MinValue;
        public int ProcessID { get; set; }
        public string? ProcessName { get; set; }
        public ServerModel? Server { get; set; }
    }
}