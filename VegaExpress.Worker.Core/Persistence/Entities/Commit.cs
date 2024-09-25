using System.ComponentModel.DataAnnotations;

namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Commit
    {
        [Key]
        public string? CommitUid { get; set; }        
        public string? RepositoryUid { get; set; }
        public string? UserUid { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Message { get; set; }
        public string? MainTreeUid { get; set; }
        public string? ParentCommitUid { get; set; }          
        public bool IsSent { get; set; }
        public virtual Repository? Repository { get; set; }
        public virtual Commit? ParentCommit { get; set; }
    }
}