using System.Collections.ObjectModel;

namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class StagingArea
    {
        public string? StagingAreaUid { get; set; }
        public string? RepositoryUid { get; set; }
        public string? PreliminaryTreeUid { get; set; }        
        public virtual Repository? Repository { get; set; }
        public virtual Tree? PreliminaryTree { get; set; }        
        public virtual ICollection<StagingAreaBlob>? StagingAreaBlobs { get; set; } // pendiente configuración en ModelBuilder
    }
}