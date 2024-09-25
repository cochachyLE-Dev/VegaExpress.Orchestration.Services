using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Data.Models
{
    public class OccurrenceModel
    {
        public OccurrenceType occurrenceType { get; set; }
        public DateTime ExecutionDate { get; set; }
        public Dictionary<string, string>? Details { get; set; }
        public string? ServiceUID { get; set; }
        public string? ServiceAgentUID { get; set; }
    }
}