using System.ComponentModel.DataAnnotations;

namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Branch
    {
        [Key]
        public string? BranchUid { get; set; }
        public string? RepositoryUId { get; set; }
        public string? Name { get; set; }
        public string? PointedCommitUid { get; set; } // Pendiente de relacionar en la configuración del Model Builder
        public virtual Repository? Repository { get; set; }
        public virtual Commit? PointedCommit { get; set; }        
    }
}