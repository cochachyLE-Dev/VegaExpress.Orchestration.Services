namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class StagingAreaBlob
    {        
        public string? StagingAreaBlobUid { get; set; }
        public string? StagingAreaUid { get; set; }
        public string? BlobUid { get; set; }
        public string? TreeUid { get; set; }
        public EntryType EntryType { get; set; }
        public ChangeType ChangeType { get; set; }        
        public virtual StagingArea? StagingArea { get; set; }
        public virtual Blob? Blob { get; set; }
        public virtual Tree? Tree { get; set; }
    }

    public enum EntryType
    {
        None = 0,
        Blob = 1,
        Tree = 2
    }
    public enum ChangeType
    {
        None = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3,
        Unchanged = 4
    }
}