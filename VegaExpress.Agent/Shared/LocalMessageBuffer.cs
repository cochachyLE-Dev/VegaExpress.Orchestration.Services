using VegaExpress.Agent.Extentions;

namespace VegaExpress.Agent.Shared
{
    public interface ILocalMessageBuffer<T>
    {
        void AddMessage(T message);
        T GetMessage();
        T GetMessage(Func<T, bool> predicate);
        bool IsEmpty();
    }
    internal class LocalMessageBuffer<T>: ILocalMessageBuffer<T>
    {
        private readonly Queue<T> _messages = new Queue<T>();
        private readonly object _lock = new object();

        public void AddMessage(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            lock (_lock)
            {
                _messages.Enqueue(message);
            }
        }

        public T GetMessage(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                if (_messages.Count == 0)
                {
                    throw new InvalidOperationException("No hay mensajes en el buffer.");
                }

                return _messages.Dequeue(predicate);
            }
        }
        public T GetMessage()
        {
            lock (_lock)
            {
                if (_messages.Count == 0)
                {
                    throw new InvalidOperationException("No hay mensajes en el buffer.");
                }

                return _messages.Dequeue();
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
}
