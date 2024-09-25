namespace VegaExpress.Agent.Data.Models
{
    public class MessageLogModel
    {
        public MessageLogModel(int row, string? message) 
        {
            Row = row;
            Message = message;
        }
        public int Row { get; set; }
        public string? Message { get; set; }
    }
}
