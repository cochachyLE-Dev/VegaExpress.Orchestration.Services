
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VegaExpress.Worker.Core.Persistence.Contracts;
using VegaExpress.Worker.Core.Persistence.Entities;

namespace VegaExpress.Worker.Core.Persistence
{
    public class RepositoryDbContext : DbContext, IRepositoryDbContext
    {
        public DbSet<Blob>? Blobs { get; set; }
        public DbSet<Branch>? Branches { get; set; }
        public DbSet<Commit>? Commits { get; set; }
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<StagingArea> StagingAreas { get; set; }
        public DbSet<StagingAreaBlob> StagingAreaBlobs { get; set; }
        public DbSet<Tree> Trees { get; set; }
        public DbSet<Pull>? Pulls { get; set; }
        public DbSet<Push>? Pushes { get; set; }
        public DbSet<User>? Users { get; set; }

        private IDbContextTransaction _currentTransaction;

        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default) => await base.SaveChangesAsync(cancellationToken);

        public async Task<IDbContextTransaction> BeginTrasactionAsync()
        {
            if (_currentTransaction != null) return null!;

            _currentTransaction = await Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentException(nameof(transaction));
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

            try
            {
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (HasActiveTransaction)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null!;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (HasActiveTransaction)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null!;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(e => e.BranchUid);

                entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(e => e.RepositoryUId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Commit>(entity =>
            {
                entity.HasKey(e => e.CommitUid);

                entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(e => e.RepositoryUid)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Tree>()
                .WithMany()
                .HasForeignKey(e => e.MainTreeUid)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Commit>()
                .WithMany()
                .HasForeignKey(e => e.ParentCommitUid)
                .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Pull>(entity =>
            {
                entity.HasKey(e => e.PullUid);

                entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(e => e.RepositoryUid)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Push>(entity =>
            {

                entity.HasKey(e => e.PushUid);

                entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(e => e.RepositoryUid)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Blob>(entity =>
            {
                entity.HasKey(e => e.BlobUid);

                entity.HasOne<Tree>()
                .WithMany()
                .HasForeignKey(e => e.TreeUid)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Repository>(entity =>
            {
                entity.HasKey(e => e.RepositoryUid);

                entity.HasOne<Commit>()
                .WithMany()
                .HasForeignKey(e => e.CurrentCommitUid)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne<Branch>()
                .WithMany()
                .HasForeignKey(e => e.CurrentBranchUid)
                .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<StagingArea>(entity =>
            {
                entity.HasKey(e => e.StagingAreaUid);

                entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(e => e.RepositoryUid)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Tree>()
                .WithMany()
                .HasForeignKey(e => e.PreliminaryTreeUid)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StagingAreaBlob>(entity =>
            {
                entity.HasKey(e => new { e.StagingAreaUid, e.BlobUid });

                entity.HasOne<StagingArea>()
                .WithMany()
                .HasForeignKey(e => e.StagingAreaUid)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tree>(entity =>
            {
                entity.HasKey(e => e.TreeUid);

                entity.HasOne<Commit>()
                .WithMany()
                .HasForeignKey(e => e.CommitUid)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserUid);
            });

            //base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=StorageDB;Username=postgres;Password=123321");

            if (!optionsBuilder.IsConfigured)
            {
            }
            base.OnConfiguring(optionsBuilder);
        }
        public RepositoryDbContext(DbContextOptions<RepositoryDbContext> options) : base(options)
        {

        }
    }
}
