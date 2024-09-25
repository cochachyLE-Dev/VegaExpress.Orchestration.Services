using Google.Protobuf.WellKnownTypes;

using System.Threading;

internal class ProcessThreadLister
{
    private readonly CancellationTokenSource cts;

    public ProcessThreadLister(CancellationTokenSource cts = null!)
    {
        this.cts = cts ?? new CancellationTokenSource();
    }

    public IEnumerable<VegaExpress.Agent.Generated.ProcessThread> Start()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(Environment.ProcessId);
            foreach (System.Diagnostics.ProcessThread pt in process.Threads)
            {
                //if (pt.WaitReason == System.Diagnostics.ThreadWaitReason.Unknown)
                {
                    var processThread = new VegaExpress.Agent.Generated.ProcessThread
                    {
                        Id = pt.Id,
                        BasePriority = pt.BasePriority,
                        CurrentPriority = pt.CurrentPriority,
                        PriorityLevel = ConvertEnumTo<System.Diagnostics.ThreadPriorityLevel, VegaExpress.Agent.Generated.ThreadPriorityLevel>(pt.PriorityLevel),
                        StartTime = ConvertDateTimeToTimestamp(pt.StartTime),
                        TotalProcessorTime = ConvertTimeSpanToDuration(pt.TotalProcessorTime),
                        UserProcessorTime = ConvertTimeSpanToDuration(pt.UserProcessorTime),
                        PrivilegedProcessorTime = ConvertTimeSpanToDuration(pt.PrivilegedProcessorTime),
                        ThreadState = ConvertEnumTo<System.Diagnostics.ThreadState, VegaExpress.Agent.Generated.ThreadState>(pt.ThreadState),
                        //WaitReason = ConvertEnumTo<System.Diagnostics.ThreadWaitReason, VegaExpress.Agent.Generated.ThreadWaitReason>(pt.WaitReason),
                    };

                    yield return processThread;
                }
            }

            Thread.Sleep(1000); // Sleep for a while before next update
        }
    }

    private TTarget ConvertEnumTo<TSource, TTarget>(TSource source) where TSource : struct, System.Enum where TTarget : struct, System.Enum
    {        
        string sourceName = source.ToString()!.ToLower();        
        var targetNames = System.Enum.GetNames(typeof(TTarget));
        
        var targetName = targetNames.FirstOrDefault(name => name.ToLower().EndsWith(sourceName));

        if (targetName == null)
        {
            throw new ArgumentException($"No matching value found in {typeof(TTarget).Name} for {source}");
        }

        if (System.Enum.TryParse(targetName, true, out TTarget target))
        {
            return target;
        }
        else
        {
            throw new ArgumentException($"Failed to parse {targetName} as a value of {typeof(TTarget).Name}");
        }
    }
    public Timestamp ConvertDateTimeToTimestamp(DateTime dateTime)
    {        
        DateTime utcDateTime = dateTime.ToUniversalTime();        
        Timestamp timestamp = Timestamp.FromDateTime(utcDateTime);

        return timestamp;
    }
    public Duration ConvertTimeSpanToDuration(TimeSpan timeSpan)
    {        
        Duration duration = Duration.FromTimeSpan(timeSpan);

        return duration;
    }

    public void Stop()
    {
        cts?.Cancel();
    }
}