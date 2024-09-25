using System.Collections.ObjectModel;

namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Tree
    {
        public string? TreeUid { get; set; }
        public string? ParentTreeUid { get; set; }
        public string? Path { get; set; }
        public long? Size { get; set; }        
        public DateTime LastModifiedDate { get; set; }
        public string? CommitUid { get; set; }
        public ICollection<Tree>? SubTrees { get; set; }
        public ICollection<Blob>? Blobs { get; set; }

        public virtual Commit? Commit { get; set; }
        public virtual Tree? ParentTree { get; set; }
    }
}
