using Grpc.Core;

namespace VegaExpress.Discovery.Services
{
    public class HealthCheckServiceClient
    {
        private readonly HealthCheckService.HealthCheckServiceClient _client;

        public HealthCheckServiceClient(HealthCheckService.HealthCheckServiceClient client)
        {
            _client = client;
        }

        public async Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            StartMonitoringRequest request = new StartMonitoringRequest();
            using var call = _client.StartMonitoring(request, cancellationToken: cancellationToken);

            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    Console.WriteLine(new string('~', 60));
                    Console.WriteLine($"Service Availability: {response.HealthCheck.Availability}");
                    Console.WriteLine($"Response Time: {response.HealthCheck.AverageResponseTime}");
                    Console.WriteLine($"CPU Usage: {response.HealthCheck.ResourceUsage["cpu"]}");
                    Console.WriteLine($"Memory Usage: {response.HealthCheck.ResourceUsage["memory"]}");
                    Console.WriteLine($"Error Rate: {response.HealthCheck.ErrorRate}");
                    Console.WriteLine($"Trafic: {response.HealthCheck.Traffic}");
                    Console.WriteLine(new string('~', 60));
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Monitoring was cancelled.");
            }
        }
    }
}
