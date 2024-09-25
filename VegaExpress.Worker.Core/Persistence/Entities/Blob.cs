using System.ComponentModel.DataAnnotations;

namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Blob
    {
        [Key]
        public string? BlobUid { get; set; }
        public string? TreeUid { get; set; }
        public string? Path { get; set; }
        public byte[]? Content { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Hash { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? CommitUid { get; set; }
        public virtual Tree? Tree { get; set; }
        public virtual Commit? Commit { get; set; }
    }
}