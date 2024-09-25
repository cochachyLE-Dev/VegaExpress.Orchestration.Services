namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Repository
    {
        public string? RepositoryUid { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
        public string? CurrentCommitUid { get; set; }
        public string? CurrentBranchUid { get; set; }

        public Branch? CurrentBranch { get; set; }
        public Commit? CurrentCommit { get; set; } 
    }
}