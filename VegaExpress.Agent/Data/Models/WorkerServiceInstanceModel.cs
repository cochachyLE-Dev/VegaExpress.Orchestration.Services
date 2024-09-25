using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Generated;

namespace VegaExpress.Agent.Data.Models
{
    public class WorkerServiceInstanceModel: IServiceInstance
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
        public string? Color { get; set; }        

        #region Bus
        public int BusPackageIn { get; set; }
        public int BusPackageOut { get; set; }
        public int BusPackageMissing { get; set; }
        #endregion

    }
}