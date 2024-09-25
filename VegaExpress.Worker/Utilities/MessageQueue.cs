using System.Collections.Concurrent;
using System.Net.Sockets;

using VegaExpress.Agent.Generated;

namespace VegaExpress.Worker.Utilities
{
    public class MessageQueue
    {
        public static MessageQueue Message = new MessageQueue();

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ConcurrentQueue<MessageRequest> queue = new ConcurrentQueue<MessageRequest>();
        //public void Send(string message, MessageRequest messageType) => queue.Enqueue(new MessageRequest { Content = message, Type = messageType });

        public async Task ProcessMessagesAsync()
        {
            await Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (queue.TryDequeue(out var message))
                    {
                        //await rpcClient.Send(message);
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            });
        }
    }  
}