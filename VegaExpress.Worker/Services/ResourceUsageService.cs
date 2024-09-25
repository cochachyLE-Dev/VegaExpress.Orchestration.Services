using VegaExpress.Worker.Services.Events;
using VegaExpress.Worker.Utilities;

namespace VegaExpress.Worker.Services
{
    public class ResourceUsageService : BackgroundBase
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly ResourceUsageMonitor _monitor;

        public ResourceUsageService(ResourceUsageMonitor monitor, ILogger<WorkerService> logger)
        {
            _monitor = monitor;
            _logger = logger;
        }        

        protected override async Task ExecuteWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Process ID: {0}", _monitor.GetProcessID());
            await _monitor.MonitorUsageAsync(stoppingToken);            
        }
    }
}
