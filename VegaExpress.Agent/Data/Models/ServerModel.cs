using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Data.Models
{
    public class ServerModel
    {
        public string? HostName { get; set; }
        public string? UserName { get; set; }
        public string? OS { get; set; }
        public string? OSArquitecture { get; set; }
        public int Processors { get; set; }
        public string? ProcessArquitecture { get; set; }
        public string? RAM { get; set; }
        public bool IsServer { get; set; }
        public bool IsClient { get; set; }
        public bool IsBlocked { get; set; }
        public NetworkAddressModel[]? NetworkAddresses { get; set; }
        public string? ServiceAgentUID { get; set; }
        public ServiceType ServiceType { get; set; }
        public string? Color { get; set; }

        public IpGeoLocationModel? IpGeoLocation { get; set; } = new IpGeoLocationModel();
    }
}
