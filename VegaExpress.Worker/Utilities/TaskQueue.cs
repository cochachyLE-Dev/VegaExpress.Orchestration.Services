using System.Collections.Concurrent;

namespace VegaExpress.Worker.Utilities
{
    public class TaskQueue
    {
        private readonly BlockingCollection<Func<Task>> queue = new BlockingCollection<Func<Task>>();
        private readonly Timer timer;
        private const int BatchSize = 10;

        public TaskQueue()
        {
            timer = new Timer(ProcessTasks!, null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
        }
        public void EnqueueTask(Func<Task> task) => queue.Add(task);

        private void ProcessTasks(object state)
        {
            var tasks = new List<Func<Task>>();

            for (int i = 0; i < BatchSize; i++)
            {
                if (queue.TryTake(out var task))
                {
                    tasks.Add(task);
                }
                else
                {
                    break;
                }
            }

            if (tasks.Count > 0)
            {
                Task.Run(async () =>
                {
                    foreach (var task in tasks)
                    {
                        await task();
                    }
                });
            }

            timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
        }
    }
}
