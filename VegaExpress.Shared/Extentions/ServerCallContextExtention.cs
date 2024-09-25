using Grpc.Core;

using Microsoft.AspNetCore.Http;

namespace VegaExpress.Worker.Extentions
{
    public static class ServerCallContextExtention
    {
        public static ConnectionInfo GetConnectionInfo(this ServerCallContext context)
        {
            var httpContext = context.GetHttpContext() ?? throw new ArgumentException(nameof(context));
            return httpContext.Connection;            
        }
    }
}