using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Data.Models
{
    public class InstructionModel
    {
        public InstructionType InstructionType { get; set; }
        public Dictionary<string, string>? Parameters { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string? ServiceUID { get; set; }
        public string? ServiceAgentUID { get; set; }
    }
}