namespace VegaExpress.Agent.Shared
{
    public class NotificationQueue
    {
        private readonly Queue<TimedMessage> _messages = new Queue<TimedMessage>();
        private readonly object _lock = new object();

        public void EnqueueMessage(string message, TimeSpan timeToLive)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var timedMessage = new TimedMessage
            {
                Message = message,
                ExpiryTime = DateTime.Now.Add(timeToLive)
            };

            lock (_lock)
            {
                _messages.Enqueue(timedMessage);
            }
        }

        public string DequeueMessage()
        {
            lock (_lock)
            {
                while (_messages.Count > 0)
                {
                    var timedMessage = _messages.Peek();
                    if (timedMessage.ExpiryTime > DateTime.UtcNow)
                    {
                        return _messages.Dequeue().Message!;
                    }
                    else
                    {
                        // Message has expired, remove it from the queue
                        _messages.Dequeue();
                    }
                }

                throw new InvalidOperationException("No messages in the queue.");
            }
        }

        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _messages.Count == 0;
            }
        }
    }
    public class TimedMessage
    {
        public string? Message { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
    public class NotificationQueue<T>
    {
        private readonly Queue<TimedMessage<T>> _messages = new Queue<TimedMessage<T>>();
        private readonly object _lock = new object();

        public void EnqueueMessage(T message, TimeSpan timeToLive)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var timedMessage = new TimedMessage<T>
            {
                Message = message,
                ExpiryTime = DateTime.Now.Add(timeToLive)
            };

            lock (_lock)
            {
                _messages.Enqueue(timedMessage);
            }
        }

        public T DequeueMessage()
        {
            lock (_lock)
            {
                while (_messages.Count > 0)
                {
                    var timedMessage = _messages.Peek();
                    if (timedMessage.ExpiryTime > DateTime.UtcNow)
                    {
                        return _messages.Dequeue().Message!;
                    }
                    else
                    {
                        // Message has expired, remove it from the queue
                        _messages.Dequeue();
                    }
                }

                throw new InvalidOperationException("No messages in the queue.");
            }
        }

        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _messages.Count == 0;
            }
        }
    }
    public class TimedMessage<T>
    {
        public T? Message { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
