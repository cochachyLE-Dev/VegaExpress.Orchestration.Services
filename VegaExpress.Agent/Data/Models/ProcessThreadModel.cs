namespace VegaExpress.Agent.Data.Models
{
    public class ProcessThreadModel
    {
        public int TaskID { get; set; }
        public int BasePriority { get; set; }
        public int CurrentPriority { get; set; }
        public System.Diagnostics.ThreadPriorityLevel PriorityLevel { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }
        public System.Diagnostics.ThreadState ThreadState { get; set; }
        public System.Diagnostics.ThreadWaitReason WaitReason { get; set; }

        public TTarget ConvertEnumTo<TSource, TTarget>(TSource source) where TSource : struct, System.Enum where TTarget : struct, System.Enum
        {
            string sourceName = source.ToString()!.ToLower();
            var targetNames = System.Enum.GetNames(typeof(TTarget));

            var targetName = targetNames.FirstOrDefault(name => name.ToLower().StartsWith(sourceName));

            if (System.Enum.TryParse(targetName, true, out TTarget target))
            {
                return target;
            }
            else
            {
                throw new ArgumentException($"Failed to parse {targetName} as a value of {typeof(TTarget).Name}");
            }
        }
    }
}
