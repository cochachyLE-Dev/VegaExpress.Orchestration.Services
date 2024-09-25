using Microsoft.EntityFrameworkCore;
using VegaExpress.Worker.Core.Persistence.Entities;

namespace VegaExpress.Worker.Core.Persistence.Contracts
{
    public interface IRepositoryDbContext
    {
        DbSet<Blob>? Blobs { get; set; }
        DbSet<Branch>? Branches { get; set; }
        DbSet<Commit>? Commits { get; set; }
        DbSet<Repository> Repositories { get; set; }
        DbSet<StagingArea> StagingAreas { get; set; }
        DbSet<StagingAreaBlob> StagingAreaBlobs { get; set; }
        DbSet<Tree> Trees { get; set; }
        DbSet<Pull>? Pulls { get; set; }
        DbSet<Push>? Pushes { get; set; }
        DbSet<User>? Users { get; set; }
        Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}