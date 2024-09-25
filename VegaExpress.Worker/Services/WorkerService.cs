using Grpc.Core;
using Grpc.Net.Client;

using Polly;

namespace VegaExpress.Worker.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly IAsyncPolicy _policy;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger, IAsyncPolicy policy)
        {
            _logger = logger;
            _policy = policy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _policy.ExecuteAsync(async () =>
            {
                // Create a channel
                var channel = GrpcChannel.ForAddress("https://localhost:5001");

                // Create a client
                var client = new ExampleService.ExampleServiceClient(channel);

                // Call Unary RPC
                var unaryResponse = await client.UnaryCallAsync(new ExampleRequest { PageIndex = 1, PageSize = 2, IsDescending = false });
                Console.WriteLine($"Unary Response: {unaryResponse.Name}");

                // Call Server Streaming RPC
                var streamingFromServerResponse = client.StreamingFromServer(new ExampleRequest { PageIndex = 1, PageSize = 2, IsDescending = false });
                await foreach (var response in streamingFromServerResponse.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"Server Streaming Response: {response.Name}");
                }

                // Call Client Streaming RPC
                var streamingFromClientRequest = client.StreamingFromClient();
                await streamingFromClientRequest.RequestStream.WriteAsync(new ExampleRequest { PageIndex = 1, PageSize = 2, IsDescending = false });
                await streamingFromClientRequest.RequestStream.CompleteAsync();
                var streamingFromClientResponse = await streamingFromClientRequest;
                Console.WriteLine($"Client Streaming Response: {streamingFromClientResponse.Name}");

                // Call Bi-directional Streaming RPC
                var streamingBothWaysRequest = client.StreamingBothWays();
                await streamingBothWaysRequest.RequestStream.WriteAsync(new ExampleRequest { PageIndex = 1, PageSize = 2, IsDescending = false });
                await streamingBothWaysRequest.RequestStream.CompleteAsync();
                await foreach (var response in streamingBothWaysRequest.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"Bi-directional Streaming Response: {response.Name}");
                }
            });
        }
    }
}
