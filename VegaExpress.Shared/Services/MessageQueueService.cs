using Grpc.Core;

using System.Collections.Concurrent;

using VegaExpress.Agent.Generated;

namespace VegaExpress.Agent.Services
{
    public class MessageQueueService : VegaExpress.Agent.Generated.MessageQueueService.MessageQueueServiceBase
    {
        private readonly IConfiguration _configuration;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private readonly ConcurrentDictionary<string, IServerStreamWriter<SubscribeResponse>> _clients = new ConcurrentDictionary<string, IServerStreamWriter<SubscribeResponse>>();        

        private readonly Queue<(string messageID, SubscribeResponse messageContent)> _queue = new Queue<(string, SubscribeResponse)>();

        private readonly ConcurrentDictionary<string, StatusType> _messageStatus = new ConcurrentDictionary<string, StatusType>();        

        public MessageQueueService(IConfiguration configuration, CancellationTokenSource cancellationTokenSource)
        {
            _configuration = configuration;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public override async Task<MessageResponse> Send(IAsyncStreamReader<MessageRequest> requestStream, ServerCallContext context)
        {
            string messageID = Guid.NewGuid().ToString();

            await foreach (var message in requestStream.ReadAllAsync())
            {
                _queue.Enqueue((messageID, new SubscribeResponse { Content = message.Content }));
                _messageStatus.TryAdd(messageID, StatusType.New);
            }
            return new MessageResponse { MessageId = messageID, Success = true };
        }

        public override Task<GetStatusResponse> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            _messageStatus.TryGetValue(request.MessageId, out StatusType status);
            var response = new GetStatusResponse { State = status };
            return Task.FromResult(response);
        }

        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<SubscribeResponse> responseStream, ServerCallContext context)
        {
            var headers = new Metadata {{ "agent-uid", _configuration.GetValue<string>("VegaExpress:Service:UID")! }};
            await context.WriteResponseHeadersAsync(headers);

            var serviceUID = context.RequestHeaders.GetValue("service-uid");
            if (!string.IsNullOrEmpty(serviceUID))
            {
                _clients.TryAdd(serviceUID, responseStream);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        _messageStatus.TryAdd(message.messageID, StatusType.Resolved);
                        await responseStream.WriteAsync(message.messageContent);
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }            
        }

        public override Task<UnsubscribeResponse> Unsubscribe(UnsubscribeRequest request, ServerCallContext context)
        {
            var serviceUID = context.RequestHeaders.GetValue("service-uid");
            if (!string.IsNullOrEmpty(serviceUID))
            {
                var success = _clients.TryRemove(serviceUID, out _);
                var response = new UnsubscribeResponse { Success = success };
                return Task.FromResult(response);
            }
            else
            {
                var response = new UnsubscribeResponse { Success = false, Message = "Service UID cannot be null!" };
                return Task.FromResult(response);
            }
        }
    }
}
