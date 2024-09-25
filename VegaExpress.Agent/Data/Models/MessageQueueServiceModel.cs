namespace VegaExpress.Agent.Data.Models
{
    public class MessageQueueServiceModel
    {
        public string? UID { get; set; }
        public string? Name { get; set; }
        public int SubscriptorsCount { get; set; }
        public string? ServiceAgentUID { get; set; }

        public IEnumerable<string>? Subscriptors { get; set; }
    }
}