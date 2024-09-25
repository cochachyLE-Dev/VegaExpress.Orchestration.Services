namespace VegaExpress.Worker.Core.Models.Settings
{
    public class MailRequest
    {
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? From { get; set; }
    }
}