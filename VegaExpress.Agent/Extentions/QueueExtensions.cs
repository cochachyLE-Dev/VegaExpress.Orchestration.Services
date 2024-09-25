namespace VegaExpress.Agent.Extentions
{
    public static class QueueExtensions
    {
        public static T Dequeue<T>(this Queue<T> queue, Func<T, bool> predicate)
        {
            T item = default(T)!;
            int count = queue.Count;

            for (int i = 0; i < count; i++)
            {
                item = queue.Dequeue();

                if (predicate(item))
                {
                    return item;
                }

                queue.Enqueue(item);
            }

            return default(T)!;
        }
    }
}
