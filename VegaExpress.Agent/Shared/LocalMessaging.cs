using System.Collections.Concurrent;

namespace VegaExpress.Agent.Shared
{
    public delegate void SubscribeEventHandler<T, TEnum>(T item, TEnum @enum) where TEnum : Enum;
    public class LocalMessaging<T>
    {        
        private readonly Timer timer;

        private ILocalMessageBuffer<(T, MessageAction)> _messageBuffer;
        private ConcurrentDictionary<string, SubscribeEventHandler<T, MessageAction>> _subscribers = new ConcurrentDictionary<string, SubscribeEventHandler<T, MessageAction>>();

        public LocalMessaging()
        {
            _messageBuffer = new LocalMessageBuffer<(T, MessageAction)>();
            timer = new Timer(ProcessQueue!, null, TimeSpan.FromMilliseconds(50), Timeout.InfiniteTimeSpan);
        }

        public void Subscribe(string uid, Action<T, MessageAction> handler)
        {
            _subscribers.TryAdd(uid, handler.Invoke);
        }
        public void Subscribe(string uid, Action<T> handler)
        {
            _subscribers.TryAdd(uid, (x,_) => handler.Invoke(x));            
        }

        public void Unsubscribe(string uid)
        {
            _subscribers.TryRemove(uid, out _);
        }

        public void SendMessage(T message)
        {
            lock (this)
            {
                _messageBuffer!.AddMessage((message, MessageAction.None));
            }
        }
        public void SendMessage(T message, MessageAction action)
        {
            lock (this)
            {
                _messageBuffer!.AddMessage((message, action));
            }
        }

        private void ProcessQueue(object state) {

            if (_subscribers.Any())
            {
                foreach (var subscriber in _subscribers)
                {                    
                    if (!_messageBuffer.IsEmpty())
                    {
                        (T, MessageAction)? message = _messageBuffer.GetMessage();
                        subscriber.Value.Invoke(message.Value.Item1, message.Value.Item2);
                    }                    
                }
            }

            timer.Change(TimeSpan.FromMilliseconds(50), Timeout.InfiniteTimeSpan);
        }

        ~LocalMessaging(){
            _subscribers = null!;
        }

    }
    public enum MessageAction
    {
        None = 0,
        Add = 1,
        Edit = 2,
        Remove = 3
    }        
}
