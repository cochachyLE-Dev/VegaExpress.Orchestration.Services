using Grpc.Core;

using VegaExpress.Agent.Generated;

namespace VegaExpress.Worker.Services
{
    public class ProcessThreadService : VegaExpress.Agent.Generated.ProcessThreadService.ProcessThreadServiceBase
    {
        public override async Task StreamProcessThreads(ProcessThreadRequest request, IServerStreamWriter<ProcessThread> responseStream, ServerCallContext context)
        {   
            ProcessThreadLister processThreadLister = new ProcessThreadLister(CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken));
            
            foreach (var processThread in processThreadLister.Start())
            {
                await responseStream.WriteAsync(processThread);
            }
        }
    }
}
