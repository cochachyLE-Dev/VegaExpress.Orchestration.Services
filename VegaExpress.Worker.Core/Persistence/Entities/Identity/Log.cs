namespace VegaExpress.Worker.Core.Persistence.Entities.Auth
{
    public class Log
    {
        public string? Id { get; set; }
        public string? AppId { get; set; }
        public string? AppVersion { get; set; }
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        public string? AccessId { get; set; }
        public string? Hostname { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinalizedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
