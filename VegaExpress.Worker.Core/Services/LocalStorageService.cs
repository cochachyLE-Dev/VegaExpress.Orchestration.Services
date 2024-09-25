using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VegaExpress.Worker.Core.Persistence.Contracts;
using VegaExpress.Worker.Core.Persistence.Entities;
using Blob = VegaExpress.Worker.Core.Persistence.Entities.Blob;

namespace VegaExpress.Worker.Core.Services
{
    public interface ILocalStorageService
    {
        Task InitRepository(string repositoryUID);
        Task CloneRepository(string sourceRepositoryUID, string targetRepositoryUID);
        Task Add(string path);
        Task PushChanges();
        Task PullChanges();
    }
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IRepositoryDbContext _repositoryDbContext;
        public LocalStorageService(IRepositoryDbContext repositoryDbContext)
        {
            _repositoryDbContext = repositoryDbContext;
        }

        public async Task InitRepository(string repositoryUID)
        {
            // Crear un nuevo repositorio
            var repository = new Repository
            {
                RepositoryUid = repositoryUID,
                CurrentCommitUid = null,
                CurrentBranchUid = null,
            };

            _repositoryDbContext.Repositories.Add(repository);
            await _repositoryDbContext.SaveEntitiesAsync();

            // Crear una rama principal para el repositorio
            var masterBranch = new Branch
            {
                BranchUid = "main",
                RepositoryUId = repositoryUID
            };

            _repositoryDbContext.Branches!.Add(masterBranch);

            await _repositoryDbContext.SaveEntitiesAsync();

            // Crear un commit inicial vacío
            var initialCommit = new Commit
            {
                CommitUid = Guid.NewGuid().ToString(),
                RepositoryUid = repositoryUID,
                UserUid = "initial_user", // asume un usuario inicial,
                MainTreeUid = null,
                ParentCommitUid = null,
            };

            _repositoryDbContext.Commits!.Add(initialCommit);
            await _repositoryDbContext.SaveEntitiesAsync();

            // Asignar el commit inicial a la rama principal
            repository.CurrentCommitUid = initialCommit.CommitUid;
            repository.CurrentBranchUid = masterBranch.BranchUid;

            _repositoryDbContext.Repositories.Update(repository);
            await _repositoryDbContext.SaveEntitiesAsync();

            // Crear un área de stagin vacía
            var stagingArea = new StagingArea
            {
                StagingAreaUid = Guid.NewGuid().ToString(),
                RepositoryUid = repositoryUID,
                PreliminaryTreeUid = null
            };
            _repositoryDbContext.StagingAreas.Add(stagingArea);
            await _repositoryDbContext.SaveEntitiesAsync();
        }

        public async Task CloneRepository(string sourceRepositoryUID, string targetRepositoryUID)
        {
            // Obtener el repositorio fuente
            var sourceRepo = await _repositoryDbContext.Repositories.FindAsync(sourceRepositoryUID);
            if (sourceRepo == null)
            {
                throw new Exception("Source repository not found.");
            }

            // Crear repositorio destino
            var targetRepo = new Repository
            {
                RepositoryUid = targetRepositoryUID,
                CurrentCommitUid = sourceRepo.CurrentCommitUid,
                CurrentBranchUid = sourceRepo.CurrentBranchUid
            };

            _repositoryDbContext.Repositories.Add(targetRepo);
            await _repositoryDbContext.SaveEntitiesAsync();

            // Clonar ramas
            var branches = _repositoryDbContext.Branches!.Where(b => b.RepositoryUId == sourceRepositoryUID).ToList();
            foreach (var branch in branches)
            {
                var newBranch = new Branch
                {
                    BranchUid = branch.BranchUid,
                    RepositoryUId = branch.RepositoryUId
                };

                _repositoryDbContext.Branches!.Add(newBranch);
            }

            // Clonar commits
            var commits = _repositoryDbContext.Commits!.Where(c => c.RepositoryUid == sourceRepositoryUID).ToList();
            foreach (var commit in commits)
            {
                var newCommit = new Commit
                {
                    CommitUid = commit.CommitUid,
                    RepositoryUid = commit.RepositoryUid,
                    UserUid = commit.UserUid,
                    MainTreeUid = commit.MainTreeUid,
                    ParentCommitUid = commit.ParentCommitUid,
                };
                _repositoryDbContext.Commits!.Add(newCommit);

                // Clonar blobs
                var blobs = _repositoryDbContext.Blobs!.Where(b => b.TreeUid == commit.MainTreeUid).ToList();
                foreach (var blob in blobs)
                {
                    var newBlob = new Blob
                    {
                        BlobUid = blob.BlobUid,
                        TreeUid = commit.MainTreeUid
                    };
                    _repositoryDbContext.Blobs!.Add(newBlob);
                }
            }

            // Clonar staging areas y blobs asociados
            var stagingAreas = _repositoryDbContext.StagingAreas.Where(sa => sa.RepositoryUid == sourceRepositoryUID).ToList();
            foreach (var stagingarea in stagingAreas)
            {
                var newStagingArea = new StagingArea
                {
                    StagingAreaUid = stagingarea.StagingAreaUid,
                    RepositoryUid = stagingarea.RepositoryUid,
                    PreliminaryTreeUid = stagingarea.PreliminaryTreeUid
                };

                _repositoryDbContext.StagingAreas.Add(newStagingArea);

                var stagingAreaBlobs = _repositoryDbContext.StagingAreaBlobs.Where(sab => sab.StagingAreaUid == stagingarea.StagingAreaUid).ToList();
                foreach (var sab in stagingAreaBlobs)
                {
                    var newStaginAreaBlob = new StagingAreaBlob
                    {
                        StagingAreaBlobUid = sab.StagingAreaBlobUid,
                        StagingAreaUid = newStagingArea.StagingAreaUid,
                        BlobUid = sab.BlobUid,
                    };
                    _repositoryDbContext.StagingAreaBlobs.Add(newStaginAreaBlob);
                }
            }

            // Guardar cambios en la base de datos
            await _repositoryDbContext.SaveEntitiesAsync();
        }

        public async Task Add(string path)
        {
            var repo = await _repositoryDbContext.Repositories.FindAsync(GetRepositoryUID());

            // Obtén la referencia del arbol actual
            var currentBranch = await _repositoryDbContext.Branches!.FindAsync(repo!.CurrentBranchUid);
            var currentCommit = await _repositoryDbContext.Commits!.FindAsync(repo!.CurrentCommitUid);
            var currentTree = await _repositoryDbContext.Trees.FindAsync(currentCommit!.MainTreeUid);

            var stagingArea = await _repositoryDbContext.StagingAreas!.FirstOrDefaultAsync(sa => sa.RepositoryUid == repo.RepositoryUid);

            if (stagingArea == null)
            {
                // Crear un nuevo staging area y añadir el blob a este staging area
                stagingArea = new StagingArea
                {
                    StagingAreaUid = Guid.NewGuid().ToString(),
                    RepositoryUid = repo.RepositoryUid,
                    PreliminaryTreeUid = currentCommit.MainTreeUid
                };

                _repositoryDbContext.StagingAreas.Add(stagingArea);
            }

            if (File.Exists(path))
            {
                await AddFileToStagingArea(path, repo.RepositoryUid!);
            }
            else if (Directory.Exists(path))
            {
                await AddDirectoryToStagingArea(path, repo.RepositoryUid!);
            }
            else
            {
                throw new FileNotFoundException($"La ruta {path} no es un archivo ni un directorio válido.");
            }

            async Task AddFileToStagingArea(string filePath, string repositoryUID)
            {
                // Obtener el blob actual (si existe) del StagingArea o del último Commit
                var currentBlob = await GetBlobFromStagingOrCommit(filePath, repositoryUID);

                // Obtener datos del archivo actual
                byte[] fileContent = File.ReadAllBytes(filePath);
                long fileSize = new FileInfo(filePath).Length;
                string fileHash = ComputeFileHash(fileContent);
                DateTime fileLastModified = File.GetLastWriteTime(filePath);

                ChangeType changeType = DetermineChangeType(currentBlob, fileHash);

                switch (changeType)
                {
                    case ChangeType.Added:
                        await AddBlobToStagingArea(filePath, fileHash, fileContent, fileSize, fileLastModified, repositoryUID);
                        break;
                    case ChangeType.Modified:
                        await UpdateBlobInStagingArea(filePath, fileHash, fileContent, fileSize, fileLastModified, repositoryUID);
                        break;
                    case ChangeType.Deleted:
                        await RemoveBlobFromStagingArea(filePath, repositoryUID);
                        break;
                    case ChangeType.Unchanged:
                        break;
                }

                async Task AddBlobToStagingArea(string filePath, string fileHash, byte[] fileContent, long fileSize, DateTime fileLastModified, string repositoryUID)
                {
                    string fileContentType = GetContentType(filePath);

                    var newBlob = new Blob
                    {
                        BlobUid = Guid.NewGuid().ToString(),
                        Path = filePath,
                        Content = fileContent,
                        ContentType = fileContentType,
                        Size = fileSize,
                        Hash = fileHash,
                        LastModifiedDate = fileLastModified,
                        TreeUid = currentCommit!.MainTreeUid,
                        CommitUid = repo.CurrentCommitUid
                    };

                    var stagingAreaBlob = new StagingAreaBlob
                    {
                        StagingAreaBlobUid = Guid.NewGuid().ToString(),
                        StagingAreaUid = stagingArea.StagingAreaUid,
                        BlobUid = newBlob.BlobUid,
                        EntryType = EntryType.Blob,
                        ChangeType = changeType
                    };

                    // Añadir el staging area y el blob a la base de datos                
                    _repositoryDbContext.Blobs!.Add(newBlob);
                    _repositoryDbContext.StagingAreaBlobs.Add(stagingAreaBlob);

                    await _repositoryDbContext.SaveEntitiesAsync();
                }
                async Task UpdateBlobInStagingArea(string filePath, string fileHash, byte[] fileContent, long fileSize, DateTime fileLastModified, string repositoryUID)
                {
                    // Actualiza el blob existente en el StagingArea
                    var stagingBlob = await GetBlobFromStagingArea(filePath, repositoryUID);

                    if (stagingBlob != null)
                    {
                        var blob = await _repositoryDbContext.Blobs!.FindAsync(stagingBlob.BlobUid);
                        blob!.Hash = fileHash;
                        blob.Content = fileContent;
                        blob.Size = fileSize;
                        blob.LastModifiedDate = fileLastModified;

                        await _repositoryDbContext.SaveEntitiesAsync();
                    }
                }
                async Task RemoveBlobFromStagingArea(string filePath, string repositoryUID)
                {
                    var stagingBlob = await _repositoryDbContext.StagingAreaBlobs
                        .Include(sab => sab.Blob)
                        .Include(sab => sab.StagingArea)
                        .FirstOrDefaultAsync(sab => sab.StagingArea!.RepositoryUid == repositoryUID && sab.Blob!.Path == filePath);

                    if (stagingBlob != null)
                    {
                        var blobUID = stagingBlob.BlobUid;

                        // Eliminar el StagingBlob
                        _repositoryDbContext.StagingAreaBlobs.Remove(stagingBlob);

                        // Verificar si el Blob está referenciado en otros lugares antes de eliminarlo
                        var isBlobReferenced = await _repositoryDbContext.StagingAreaBlobs.AnyAsync(sb => sb.BlobUid == blobUID);

                        if (!isBlobReferenced)
                        {
                            var blob = await _repositoryDbContext.Blobs!.SingleOrDefaultAsync(b => b.BlobUid == blobUID);
                            if (blob != null)
                            {
                                _repositoryDbContext.Blobs!.Remove(blob);
                            }
                        }

                        await _repositoryDbContext.SaveEntitiesAsync();
                    }
                }
                async Task<Blob> GetBlobFromStagingOrCommit(string filePath, string repositoryUID)
                {
                    // Buscar en el stagingArea
                    var stagingBlob = await GetBlobFromStagingArea(filePath, repositoryUID);

                    if (stagingBlob != null)
                        return stagingBlob;

                    // Buscar en el último Commit
                    var lastCommitBlob = await GetBlobFromLastCommit(filePath, repositoryUID);

                    return lastCommitBlob;
                }
                async Task<Blob> GetBlobFromStagingArea(string filePath, string repositoryUID)
                {
                    // Buscar en el StagingAreaBlob por el archivo en la ruta específica y el repositorio
                    var stagingBlob = await _repositoryDbContext.StagingAreaBlobs
                        .Include(sab => sab.Blob)
                        .Include(sab => sab.StagingArea)
                        .FirstOrDefaultAsync(sab => sab.StagingArea!.RepositoryUid == repositoryUID && sab.Blob!.Path == filePath);

                    // Si existe un blob en el StagingArea, retornar el Blob
                    if (stagingBlob != null)
                    {
                        return stagingBlob.Blob!;
                    }

                    return null!;
                }
                async Task<Blob> GetBlobFromLastCommit(string filePath, string repositoryUID)
                {
                    var lastCommit = await _repositoryDbContext.Commits!
                        .Where(c => c.RepositoryUid == repositoryUID)
                        .OrderByDescending(c => c.CommitUid)
                        .FirstOrDefaultAsync();

                    // Si no hay commits, retornar null
                    if (lastCommit == null)
                        return null!;

                    // Obtener el árbol (Tree) principal del commit
                    var mainTree = await _repositoryDbContext.Trees
                        .FirstOrDefaultAsync(t => t.TreeUid == lastCommit.MainTreeUid);

                    if (mainTree == null)
                        return null!;

                    // Buscar el Blob en el árbol principal que corresponde al archivo (filePath)
                    var blob = await _repositoryDbContext.Blobs!
                        .Include(b => b.Tree)
                        .FirstOrDefaultAsync(b => b.TreeUid == mainTree.TreeUid && b.Path == filePath);

                    // retornar el Blob si se encuentra
                    return blob!;
                }
                ChangeType DetermineChangeType(Blob currentBlob, string newFileHash)
                {
                    if (currentBlob == null)
                    {
                        // Si no existe en el StagingArea o en el último Commit, es un archivo nuevo (Added).
                        return ChangeType.Added;
                    }

                    // Si el hash del archivo ha cambiado, es un archivo modificado (Modified)
                    if (currentBlob.Hash != newFileHash)
                    {
                        return ChangeType.Modified;
                    }

                    // Si no ha cambiado, entonces no hay cambios (Unchanged)
                    return ChangeType.Unchanged;
                }
                string GetContentType(string filePath)
                {
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(filePath, out string contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    return contentType;
                }
                string ComputeFileHash(byte[] content)
                {
                    using (var sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(content);
                        // convert hash bytes to hexadecimal string
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            async Task AddDirectoryToStagingArea(string directoryPath, string repositoryUID)
            {
                // Obtener el blob actual (si existe) del StagingArea o del último Commit
                var currentBlob = await GetBlobFromStagingOrCommit(directoryPath, repositoryUID);

                // Obtener datos del archivo actual           
                long directorySize = GetDirectorySize(directoryPath);
                DateTime directoryLastModified = Directory.GetLastWriteTime(directoryPath);

                ChangeType changeType = DetermineChangeType(currentBlob, directorySize, directoryLastModified);

                switch (changeType)
                {
                    case ChangeType.Added:
                        await AddBlobToStagingArea(directoryPath, directorySize, directoryLastModified, repositoryUID);
                        break;
                    case ChangeType.Modified:
                        await UpdateBlobInStagingArea(directoryPath, directorySize, directoryLastModified, repositoryUID);
                        break;
                    case ChangeType.Deleted:
                        await RemoveBlobFromStagingArea(directoryPath, repositoryUID);
                        break;
                }

                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    await AddFileToStagingArea(file, repositoryUID);
                }

                var directories = Directory.GetDirectories(directoryPath);
                foreach (var directory in directories)
                {
                    await AddDirectoryToStagingArea(directory, repositoryUID);
                }

                async Task AddBlobToStagingArea(string directoryPath, long directorySize, DateTime directoryLastModified, string repositoryUID)
                {
                    var newBlob = new Tree
                    {
                        TreeUid = Guid.NewGuid().ToString(),
                        Path = directoryPath,
                        Size = directorySize,
                        LastModifiedDate = directoryLastModified,
                        ParentTreeUid = currentCommit!.MainTreeUid,
                        CommitUid = repo.CurrentCommitUid
                    };

                    var stagingAreaBlob = new StagingAreaBlob
                    {
                        StagingAreaBlobUid = Guid.NewGuid().ToString(),
                        StagingAreaUid = stagingArea.StagingAreaUid,
                        TreeUid = newBlob.TreeUid,
                        EntryType = EntryType.Tree,
                        ChangeType = changeType
                    };

                    // Añadir el staging area y el blob a la base de datos                
                    _repositoryDbContext.Trees!.Add(newBlob);
                    _repositoryDbContext.StagingAreaBlobs.Add(stagingAreaBlob);

                    await _repositoryDbContext.SaveEntitiesAsync();
                }
                async Task UpdateBlobInStagingArea(string directoryPath, long directorySize, DateTime directoryLastModified, string repositoryUID)
                {
                    // Actualiza el blob existente en el StagingArea
                    var stagingBlob = await GetBlobFromStagingArea(directoryPath, repositoryUID);

                    if (stagingBlob != null)
                    {
                        var blob = await _repositoryDbContext.Trees!.FindAsync(stagingBlob.TreeUid);
                        blob!.Size = directorySize;
                        blob.LastModifiedDate = directoryLastModified;

                        await _repositoryDbContext.SaveEntitiesAsync();
                    }
                }
                async Task RemoveBlobFromStagingArea(string directoryPath, string repositoryUID)
                {
                    var stagingBlob = await _repositoryDbContext.StagingAreaBlobs
                        .Include(sab => sab.Tree)
                        .Include(sab => sab.StagingArea)
                        .FirstOrDefaultAsync(sab => sab.StagingArea!.RepositoryUid == repositoryUID && sab.Tree!.Path == directoryPath);

                    if (stagingBlob != null)
                    {
                        // Obtener el ParentTree correspondiente
                        var tree = await _repositoryDbContext.Trees
                            .Include(t => t.Blobs)
                            .Include(t => t!.SubTrees)
                            .FirstOrDefaultAsync(t => t.TreeUid == stagingBlob.TreeUid);

                        if (tree == null)
                        {
                            throw new InvalidOperationException("ParentTree no encontrado.");
                        }

                        // Eliminar todos los blobs asociados al ParentTree del StagingArea
                        foreach (var blob in tree.Blobs!)
                        {
                            var stagingAreaBlob = stagingArea!.StagingAreaBlobs!
                                .FirstOrDefault(sb => sb.BlobUid == blob.BlobUid);

                            if (stagingAreaBlob != null)
                            {
                                _repositoryDbContext.StagingAreaBlobs.Remove(stagingAreaBlob);
                            }
                        }

                        // Eliminar todos los subárboles (trees) asociados al ParentTree del StagingArea
                        foreach (var subTree in tree.SubTrees!)
                        {
                            await RemoveBlobFromStagingArea(subTree.Path!, repositoryUID);
                        }

                        #region Remove ParentTree
                        // Eliminar el StagingBlob
                        _repositoryDbContext.StagingAreaBlobs.Remove(stagingBlob);

                        // Verificar si el Blob está referenciado en otros lugares antes de eliminarlo
                        var isBlobReferenced = await _repositoryDbContext.StagingAreaBlobs.AnyAsync(sb => sb.TreeUid == stagingBlob.TreeUid);

                        if (!isBlobReferenced)
                        {
                            var blob = await _repositoryDbContext.Trees!.SingleOrDefaultAsync(b => b.TreeUid == stagingBlob.TreeUid);
                            if (blob != null)
                            {
                                _repositoryDbContext.Trees!.Remove(blob);
                            }
                        }

                        await _repositoryDbContext.SaveEntitiesAsync();
                        #endregion
                    }
                }
                async Task<Tree> GetBlobFromStagingOrCommit(string filePath, string repositoryUID)
                {
                    // Buscar en el stagingArea
                    var stagingBlob = await GetBlobFromStagingArea(filePath, repositoryUID);

                    if (stagingBlob != null)
                        return stagingBlob;

                    // Buscar en el último Commit
                    var lastCommitBlob = await GetBlobFromLastCommit(filePath, repositoryUID);

                    return lastCommitBlob;
                }
                async Task<Tree> GetBlobFromStagingArea(string directoryPath, string repositoryUID)
                {
                    // Buscar en el StagingAreaBlob por el archivo en la ruta específica y el repositorio
                    var stagingBlob = await _repositoryDbContext.StagingAreaBlobs
                        .Include(sab => sab.Tree)
                        .Include(sab => sab.StagingArea)
                        .FirstOrDefaultAsync(sab => sab.StagingArea!.RepositoryUid == repositoryUID && sab.Tree!.Path == directoryPath);

                    // Si existe un blob en el StagingArea, retornar el Blob
                    if (stagingBlob != null)
                    {
                        return stagingBlob.Tree!;
                    }

                    return null!;
                }
                async Task<Tree> GetBlobFromLastCommit(string directoryPath, string repositoryUID)
                {
                    var lastCommit = await _repositoryDbContext.Commits!
                        .Where(c => c.RepositoryUid == repositoryUID)
                        .OrderByDescending(c => c.CommitUid)
                        .FirstOrDefaultAsync();

                    // Si no hay commits, retornar null
                    if (lastCommit == null)
                        return null!;

                    // Obtener el árbol (Tree) principal del commit
                    var mainTree = await _repositoryDbContext.Trees
                        .FirstOrDefaultAsync(t => t.TreeUid == lastCommit.MainTreeUid);

                    if (mainTree == null)
                        return null!;

                    // Buscar el Blob en el árbol principal que corresponde al archivo (filePath)
                    var tree = await _repositoryDbContext.Trees!
                        .Include(t => t.ParentTree)
                        .FirstOrDefaultAsync(t => t.TreeUid == mainTree.TreeUid && t.Path == directoryPath);

                    // retornar el Blob si se encuentra
                    return tree!;
                }
                ChangeType DetermineChangeType(Tree currentBlob, long size, DateTime lastModifiedDate)
                {
                    if (currentBlob == null)
                    {
                        // Si no existe en el StagingArea o en el último Commit, es un archivo nuevo (Added).
                        return ChangeType.Added;
                    }

                    // Si el tamaño del directorio ha cambiado, es un archivo modificado (Modified)
                    if (currentBlob.Size != size || currentBlob.LastModifiedDate != lastModifiedDate)
                    {
                        return ChangeType.Modified;
                    }

                    // Si no ha cambiado, entonces no hay cambios (Unchanged)
                    return ChangeType.Unchanged;
                }
                long GetDirectorySize(string directoryPath)
                {
                    // Verificar si el directorio existe
                    if (!Directory.Exists(directoryPath))
                    {
                        throw new DirectoryNotFoundException($"El directorio {directoryPath} no existe.");
                    }

                    long totalSize = 0;

                    // Obtener todos los archivos en el directorio y sumar sus tamaños
                    string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        // Obtener información del archivo
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }

                    return totalSize;
                }
            }
        }

        public async Task CommitChanges(string message)
        {
            // Obtener el repositorio actual
            var repositoryUID = GetRepositoryUID();
            var repository = await _repositoryDbContext.Repositories
                .Include(r => r.CurrentBranch)
                .ThenInclude(b => b!.PointedCommit)
                .FirstOrDefaultAsync(r => r.RepositoryUid == repositoryUID);

            if (repository == null)
            {
                throw new InvalidOperationException("Repository not found.");
            }

            // Obtener el usuario actual
            var userUID = GetCurrentUserUID();
            var author = await _repositoryDbContext.Users!.FindAsync(userUID);
            if (author == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Obtener el stagingArea correspondiente
            var stagingArea = await _repositoryDbContext.StagingAreas
                .Include(sa => sa.StagingAreaBlobs)!
                .ThenInclude(sb => sb.Blob)
                .Include(sa => sa.StagingAreaBlobs)!
                .ThenInclude(sb => sb.Tree)
                .Include(sa => sa.PreliminaryTree)
                .FirstOrDefaultAsync(sa => sa.RepositoryUid == repositoryUID);

            if (stagingArea == null)
            {
                throw new InvalidOperationException("StagingArea not found.");
            }

            // Crear un nuevo Tree para el commit
            var newTree = stagingArea.PreliminaryTree!;

            // Crear el nuevo commit
            var newCommit = new Commit
            {
                CommitUid = Guid.NewGuid().ToString(),
                RepositoryUid = repository.RepositoryUid,
                UserUid = author.UserUid,
                MainTreeUid = newTree.TreeUid,
                ParentCommitUid = repository.CurrentBranch!.PointedCommitUid,
                Message = message,
                DateCreated = DateTime.UtcNow
            };

            // Asociar blobs del StagingArea al nuevo Tree
            foreach (var stagingBlob in stagingArea.StagingAreaBlobs!)
            {
                switch (stagingBlob.EntryType)
                {
                    case EntryType.Blob:
                        {
                            var blob = new Blob
                            {
                                BlobUid = Guid.NewGuid().ToString(),
                                TreeUid = newTree.TreeUid,
                                Path = stagingBlob.Blob!.Path,
                                Content = stagingBlob.Blob.Content,
                                ContentType = stagingBlob.Blob.ContentType,
                                Size = stagingBlob.Blob.Size,
                                LastModifiedDate = stagingBlob.Blob.LastModifiedDate,
                                CommitUid = newCommit.CommitUid
                            };

                            newTree.Blobs ??= new Collection<Blob>();
                            newTree.Blobs.Add(blob);

                            _repositoryDbContext.Blobs!.Add(blob);
                        }
                        break;
                    case EntryType.Tree:
                        {
                            var tree = new Tree
                            {
                                TreeUid = Guid.NewGuid().ToString(),
                                ParentTreeUid = newTree.TreeUid,
                                Path = stagingBlob.Tree!.Path,
                                Size = stagingBlob.Tree.Size,
                                LastModifiedDate = stagingBlob.Tree.LastModifiedDate,
                                CommitUid = newCommit.CommitUid
                            };

                            newTree.SubTrees ??= new Collection<Tree>();
                            newTree.SubTrees.Add(tree);

                            _repositoryDbContext.Trees!.Add(tree);
                        }
                        break;
                }
            }

            // Actualizar el Tree con el CommitUID
            newTree.CommitUid = newCommit.CommitUid;

            // Agregar el nuevo Tree y Commit al contexto
            _repositoryDbContext.Trees.Add(newTree);
            _repositoryDbContext.Commits!.Add(newCommit);

            // Actualizar el CurrentBranch para que apunte el nuevo commit
            repository.CurrentBranch.PointedCommitUid = newCommit.CommitUid;

            // Limpiar el StagingArea
            stagingArea.StagingAreaBlobs.Clear();
            _repositoryDbContext.StagingAreas.Update(stagingArea);

            await _repositoryDbContext.SaveEntitiesAsync();
        }
        public async Task PushChanges()
        {
            var repositoryUID = GetRepositoryUID();
            var repository = await _repositoryDbContext.Repositories
                .Include(r => r.CurrentBranch)
                .FirstOrDefaultAsync(r => r.RepositoryUid == repositoryUID);

            if (repository == null)
            {
                throw new InvalidOperationException("Repository not found.");
            }

            // Obtener el usuario actual
            var userUID = GetCurrentUserUID();
            var user = await _repositoryDbContext.Users!.FirstOrDefaultAsync(u => u.UserUid == userUID);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Obtener el commit más reciente del CurrentBranch
            var latestCommit = await _repositoryDbContext.Commits!
                .Where(c => c.RepositoryUid == repositoryUID)
                .OrderByDescending(c => c.DateCreated)
                .FirstOrDefaultAsync();

            if (latestCommit == null)
            {
                throw new InvalidOperationException("No commits found to push.");
            }

            // Verificar si el último commit ya fue "pusheado" previamente (evitar duplicados)
            bool isAlreadyPushed = await _repositoryDbContext.Pushes!
                .AnyAsync(p => p.RepositoryUid == repositoryUID && p.UserUid == userUID && p.CommitUid == latestCommit.CommitUid);

            if (isAlreadyPushed)
            {
                throw new InvalidOperationException("This commit has already been pushed.");
            }

            // Crear un nuevo registro de Push
            var newPush = new Push
            {
                PushUid = Guid.NewGuid().ToString(),
                RepositoryUid = repository.RepositoryUid,
                UserUid = user.UserUid,
                CommitUid = latestCommit.CommitUid,
                DatePushed = DateTime.UtcNow
            };

            // Guardar el nuevo Push en la base de datos
            _repositoryDbContext.Pushes!.Add(newPush);

            // Push to remote server
            // await PushToRemoteServer(repository, latestCommit);

            // Guardar los cambios en la base de datos
            await _repositoryDbContext.SaveEntitiesAsync();

            async Task PushToRemoteServer(Repository repository, Commit latestCommit)
            {
                var mainTree = await _repositoryDbContext.Trees
                    .Include(t => t.Blobs)
                    .Include(t => t.SubTrees)
                    .FirstOrDefaultAsync(t => t.TreeUid == latestCommit.MainTreeUid);

                if (mainTree == null)
                {
                    throw new InvalidOperationException("Main tree for the commit not found.");
                }

                // Recopilar blobs asociados al árbol para enviar al servidor remoto
                var blobsToPush = mainTree.Blobs;

                // Simular la conexión con el servidor remoto
                Console.WriteLine($"Connecting to remote server for repository {repository.RepositoryUid}...");

                // Envío de blobs al servidor remoto
                foreach (var blob in blobsToPush!)
                {
                    var blobContent = blob.Content!;

                    // Subida del blob al servidor remoto
                    Debug.WriteLine($"Pushing blob {blob.BlobUid} to remote server...");
                    await RemoteUpload(blob, blobContent);
                }

                // Registro del commit en el servidor remoto
                Debug.WriteLine($"Pushing commit {latestCommit.CommitUid} to remote server ...");
                await RemoteCommitPush(latestCommit);

                Debug.WriteLine($"SuccessFully pushed commit {latestCommit.CommitUid} and associated blobs to remote server.");

                async Task RemoteUpload(Blob blob, byte[] content)
                {
                    Debug.WriteLine($"Uploading blob {blob.BlobUid} (size: {content.Length} bytes) to remote server...");

                    var url = "https://cluster.com/api/upload";
                    using (var client = new HttpClient())
                    {
                        var contentToSend = new ByteArrayContent(content);
                        var response = await client.PostAsync(url, contentToSend);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception("Error uploading blob to remote server.");
                        }
                    }

                    Debug.WriteLine($"Blob {blob.BlobUid} uploaded successfully.");
                }
                async Task RemoteCommitPush(Commit commit)
                {
                    // registro del commit en un servidor remoto
                    Debug.WriteLine($"Pushing commit {commit.CommitUid} to remote server...");

                    // Llamada HTTP para registrar el commit en un servidor remoto.
                    var url = "https://cluster.com/api/pushCommit";
                    using (var client = new HttpClient())
                    {
                        var commitData = JsonSerializer.Serialize(commit);
                        var contentToSend = new StringContent(commitData, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(url, contentToSend);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception("Error pushing commit to remote server.");
                        }
                    }

                    // Éxito del push de commit
                    Debug.WriteLine($"Commit {commit.CommitUid} pushed successfully.");
                }
            }
        }
        public async Task PullChanges()
        {
            // Obtener el repositorio y la rama actual
            var repositoryUID = GetRepositoryUID();
            var repository = await _repositoryDbContext.Repositories.
                Include(r => r.CurrentBranch)
                .FirstOrDefaultAsync(r => r.RepositoryUid == repositoryUID);

            if (repository == null)
            {
                throw new InvalidOperationException("Repository not found.");
            }

            var currentBranch = repository.CurrentBranch;
            if (currentBranch == null)
            {
                throw new InvalidOperationException("Current branch not found.");
            }

            // Obtener el commit más reciente en la rama actual
            var localCommit = await _repositoryDbContext.Commits!
                .Where(c => c.RepositoryUid == repositoryUID && c.CommitUid == repository.CurrentCommitUid)
                .FirstOrDefaultAsync();



            (List<Commit> Commits, List<Blob> Blobs) FetchFromRemoteServer(string repositoryUID)
            {
                var commits = new List<Commit>
                {
                };

                var blobs = new List<Blob>
                {
                };

                return (commits, blobs);
            }
            async Task ApplyCommit(Commit commit)
            {
                // Obtener el árbol principal del commit
                var tree = await _repositoryDbContext.Trees
                    .Include(t => t.Blobs)
                    .FirstOrDefaultAsync(t => t.TreeUid == commit.MainTreeUid);

                if (tree == null)
                {
                    throw new InvalidOperationException("Tree not found for commit.");
                }

                // Aplicar blobs al sistema de archivos local
                foreach (var blob in tree.Blobs)
                {
                    // Descargar el contenido del blob
                    //var blobContent
                }
            }
        }
        public string GetRepositoryUID() => "";
        public string GetCurrentUserUID() => "";
    }
}
