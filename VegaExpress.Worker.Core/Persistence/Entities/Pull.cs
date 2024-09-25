namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class Pull
    {
        public string? PullUid { get; set; }
        public string? RepositoryUid { get; set; }
        public DateTime Date { get; set; }
        public string? UserUid { get; set; }
        public string? BrandName { get; set; }
    }
}