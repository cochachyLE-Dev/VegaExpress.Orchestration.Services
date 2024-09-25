using Grpc.Core;
using Grpc.Net.Client;

namespace VegaExpress.Worker.Services
{    
    public class ExampleServiceClient: ExampleService.ExampleServiceClient
    {
        private readonly ExampleService.ExampleServiceClient _client;

        public ExampleServiceClient(string address)
        {
            var channel = GrpcChannel.ForAddress(address);
            _client = new ExampleService.ExampleServiceClient(channel);
        }

        public async Task<ExampleResponse> UnaryCallAsync(ExampleRequest request)
        {
            return await _client.UnaryCallAsync(request);
        }

        public async IAsyncEnumerable<ExampleResponse> StreamingFromServerAsync(ExampleRequest request)
        {
            var responseStream = _client.StreamingFromServer(request);
            await foreach (var item in responseStream.ResponseStream.ReadAllAsync())
            {
                yield return item;
            }
        }

        public async Task<ExampleResponse> StreamingFromClientAsync(IAsyncEnumerable<ExampleRequest> requests)
        {
            var call = _client.StreamingFromClient();
            await foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }
            await call.RequestStream.CompleteAsync();
            return await call.ResponseAsync;
        }

        public async IAsyncEnumerable<ExampleResponse> StreamingBothWaysAsync(IAsyncEnumerable<ExampleRequest> requests)
        {
            var call = _client.StreamingBothWays();
            await foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }
            await call.RequestStream.CompleteAsync();
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                yield return response;
            }
        }

    }
}
