namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Push
    {
        public string? PushUid { get; set; }
        public string? RepositoryUid { get; set; }
        public DateTime DatePushed { get; set; }
        public string? CommitUid { get; set; }
        public string? UserUid { get; set; }
        public string? BranchName { get; set; }
    }
}