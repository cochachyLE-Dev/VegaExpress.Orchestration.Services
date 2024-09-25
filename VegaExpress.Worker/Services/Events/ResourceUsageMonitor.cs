using System.Diagnostics.Tracing;
using System.Diagnostics;

namespace VegaExpress.Worker.Services.Events
{
    [EventSource(Name = nameof(ResourceUsageMonitor))]
    public class ResourceUsageMonitor : EventSource
    {
        private readonly ILogger<WorkerService> _logger;
        private EventCounter cpuCounter;
        private EventCounter memoryCounter;
        private Process process;        

        public ResourceUsageMonitor(ILogger<WorkerService> logger) : base("ResourceUsageMonitor")
        {
            process = Process.GetCurrentProcess();
            cpuCounter = new EventCounter("cpu-usage", this);
            memoryCounter = new EventCounter("memory-usage", this);
            _logger = logger;
        }

        public async Task MonitorUsageAsync(CancellationToken stoppingToken)
        {            
            while (!stoppingToken.IsCancellationRequested)
            {                
                cpuCounter.WriteMetric((float)GetCpuUsage());
                memoryCounter.WriteMetric((float)GetMemoryUsage());

                _logger.LogInformation("CPU: {0}", GetCpuUsage());
                _logger.LogInformation("Memory: {0}", GetMemoryUsage());

                // Update every second
                await Task.Delay(1000);                
            }
        }
        public int GetProcessID() => process.Id;
        private double GetCpuUsage()
        {
            var startTime = DateTime.Now;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var endTime = DateTime.Now;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100; // Returns the CPU usage as a percentage
        }

        private double GetMemoryUsage()
        {
            return process.WorkingSet64 / 1024 / 1024;
        }
    }
}
