namespace VegaExpress.Agent.Data.Models
{
    public class TaskSchedulerServiceModel
    {
        public string? TaskUID { get; set; }
        public string? TaskName { get; set; }
        public string? ServiceUID { get; set; }
        public string? ServiceAgentUID { get; set; }
        public bool TaskHasExpiration { get; set; }
        public DateTime TaskExecutionDate { get; set; }
        public DateTime TaskExpirationDate { get; set; }
        public TaskStatus TaskStatus { get; set; } = TaskStatus.Unknown;
        public bool TaskIsReadonly { get; set; }
    }
    public enum TaskType
    {
        Unknown = 0,
        Unary = 1,
        ServerStreaming = 2,
        ClientStreaming = 3,
        Bidirectional = 4      
    }
    public enum TaskStatus
    {
        Unknown = 0,
        New = 1,
        Open = 2,
        Pending = 3,
        OnHold = 4,
        InProgress = 5,
        Resolved = 6,
        Closed = 7,
        Cancelled = 8
    }
}
