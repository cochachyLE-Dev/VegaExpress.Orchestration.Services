using Grpc.Core;

using VegaExpress.Discovery;
using VegaExpress.Worker.Extentions;
using VegaExpress.Worker.Shared;

namespace VegaExpress.Worker.Services
{
    public class HealthCheckServiceServer : HealthCheckService.HealthCheckServiceBase
    {        
        public override async Task StartMonitoring(StartMonitoringRequest request, IServerStreamWriter<StartMonitoringResponse> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var response = new StartMonitoringResponse();
                try
                {
                    var healthCheck = new HealthCheck
                    {
                        Availability = GlobalServiceData.Availability,
                        AverageResponseTime = GlobalServiceData.AverageResponseTime,
                        ErrorRate = GlobalServiceData.ErrorRate,
                        Traffic = GlobalServiceData.Traffic,
                        Location = GlobalServiceData.Location
                    };
                    var attributes = new Dictionary<string, double>
                    {
                        ["cpu"] = 0,
                        ["memory"] = 0,
                    };
                    healthCheck.ResourceUsage.Add(attributes);

                    response.Status = (int)ResponseStatus.Success;
                    response.Message = ResponseStatusExtention.GetSuccessMessage();
                }
                catch (Exception ex) when (ex is RpcException)
                {
                    GlobalServiceData.ErrorRate++;
                    GlobalServiceData.Availability = false;
                    break;
                }
                catch (Exception)
                {
                    response.Status = (int)ResponseStatus.InternalServerError;
                    response.Message = ResponseStatus.InternalServerError.GetMessage();
                    break;
                }
                finally
                {
                    await responseStream.WriteAsync(response);
                    await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
                }
            }
        }        
    }
}