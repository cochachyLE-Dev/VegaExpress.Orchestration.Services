using Google.Protobuf;

using Grpc.Core;
using Grpc.Net.Client;

using MemoryPack;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using Splat.ModeDetection;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using VegaExpress.Worker.Core.Persistence.Contracts;
using VegaExpress.Worker.Core.Persistence.Entities;
using VegaExpress.Worker.Core.Utilities;
using VegaExpress.Worker.Storage.Generated;

using static Google.Rpc.Context.AttributeContext.Types;
using static System.Reflection.Metadata.BlobBuilder;

namespace VegaExpress.Worker.Client.Services
{
    public class StorageService : storage_db_service.storage_db_serviceBase
    {
        private readonly IRepositoryDbContext _repositoryDbContext;
        public StorageService(IRepositoryDbContext repositoryDbContext) {
            _repositoryDbContext = repositoryDbContext;
        } 
        public override Task clone(clone_request request, IServerStreamWriter<clone_response> responseStream, ServerCallContext context)
        {
            return base.clone(request, responseStream, context);
        }
        public override async Task<add_response> add(add_request request, ServerCallContext context)
        {
            try
            {
                if(string.IsNullOrEmpty(request.RepositoryUid) || string.IsNullOrEmpty(request.OriginUid) || (request.Key == null || request.Key.Length == 0))
                    return new add_response { Result = new add_result { Success = false, Message = "" } };

                Repository repository = await _repositoryDbContext.Repositories.Where(r => r.RepositoryUid.Equals(request.RepositoryUid)).FirstOrDefaultAsync();

                if (repository == null)
                    return new add_response { Result = new add_result { Success = false, Message = "Repository not found." } };

                Core.Persistence.Entities.Blob record = new Core.Persistence.Entities.Blob { 
                    
                };
            }
            catch
            { 
            }

            return await base.add(request, context);
        }
        public override Task<commit_response> commit(commit_request request, ServerCallContext context)
        {
            return base.commit(request, context);
        }
        public override async Task push(IAsyncStreamReader<push_request> requestStream, IServerStreamWriter<push_response> responseStream, ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            string serviceWorkerUid = headers.Get("service-uid")!.Value;
            string serviceWorkerPid = headers.Get("service-pid")!.Value;
            string serviceWorkerAddress = headers.Get("service-address")!.Value;
            
            var repositoryUid = headers.Get("repository-uid")!.Value;
            var commitsToPush = _repositoryDbContext.Commits!.Where(c => !c.IsSent);

            var blobs = from b in _repositoryDbContext.Blobs
                        join t in _repositoryDbContext.Trees on b.TreeUid equals t.TreeUid
                        join c in _repositoryDbContext.Commits! on t.TreeUid equals c.MainTreeUid
                        where c.RepositoryUid == repositoryUid && commitsToPush.Select(c => c.CommitUid).Contains(c.CommitUid)
                        select b;

            var commits = await commitsToPush.ToArrayAsync();

            ConcurrentQueue<Core.Persistence.Entities.Blob> blobQueue = new ConcurrentQueue<Core.Persistence.Entities.Blob>(blobs);

            while (await requestStream.MoveNext())
            {
                if (requestStream.Current.Message.StartsWith("+"))
                {
                    _ = ChangeBlobStatus(requestStream.Current);

                    if (blobQueue.TryDequeue(out var blob))
                    {
                        //ByteString keyByteString = ByteString.CopyFrom(blob.Key);
                        ByteString contentByteString = ByteString.CopyFrom(blob.Content);

                        var response = new push_response()
                        {
                            Data = new push_data
                            {
                                TreeUid = blob.TreeUid,
                                CommitUid = blob.Tree!.CommitUid,
                                BlobUid = blob.BlobUid,
                                Changes = new changes()
                                {
                                    //OriginUid = blob.OriginUid,
                                    //Key = keyByteString,
                                    Record = contentByteString
                                }
                            }
                        };

                        await responseStream.WriteAsync(response);
                    }
                }
                else if (requestStream.Current.Message == "commit")
                {
                    var commit = commits.Single(c => c.CommitUid == requestStream.Current.CommitUid);
                    var response = new push_response()
                    {
                        Comit = new push_commit
                        {
                            CommitUid = commit.CommitUid,
                            RepositoryUid = commit.RepositoryUid,
                            UserUid = commit.UserUid,
                            Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(commit.DateCreated.ToUniversalTime()),
                            Message = commit.Message,
                            MainTreeUid = commit.MainTreeUid,
                            ParentCommitUid = commit.ParentCommitUid                            
                        }
                    };

                    await responseStream.WriteAsync(response);
                }
            }

            for (int i = 0; i < commits.Length; i++)
            { 
                if (!blobs.Any(b => b.TreeUid == commits[i].MainTreeUid))
                {
                    commits[i].IsSent = true;
                }
            }
            await _repositoryDbContext.SaveEntitiesAsync();

            async Task ChangeBlobStatus(push_request request)
            {
                if (string.IsNullOrEmpty(request.TreeUid) || string.IsNullOrEmpty(request.CommitUid) || string.IsNullOrEmpty(request.BlobUid)) return;

                var blob = blobs.Single(c => c.BlobUid == request.BlobUid);
                //blob.IsSent = true;

                await _repositoryDbContext.SaveEntitiesAsync();
            }
        }

        public override async Task pull(IAsyncStreamReader<pull_request> requestStream, IServerStreamWriter<pull_response> responseStream, ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            string serviceWorkerUid = headers.Get("service-uid")!.Value;
            string serviceWorkerPid = headers.Get("service-pid")!.Value;
            string serviceWorkerAddress = headers.Get("service-address")!.Value;

            var userUid = headers.Get("user-uid")!.Value;
            var repositoryUid = headers.Get("repository-uid")!.Value;

            var lastPullDate = (from p in _repositoryDbContext.Pulls
                                where p.RepositoryUid == repositoryUid
                                orderby p.Date descending
                                select p.Date).FirstOrDefault();

            var commitsToPull = from c in _repositoryDbContext.Commits
                                where c.RepositoryUid == repositoryUid && c.DateCreated > lastPullDate
                                select c;

            var blobs = await (from b in _repositoryDbContext.Blobs
                        join t in _repositoryDbContext.Trees on b.TreeUid equals t.TreeUid
                        join c in _repositoryDbContext.Commits! on t.TreeUid equals c.MainTreeUid
                        where c.RepositoryUid == repositoryUid && commitsToPull.Select(c => c.CommitUid).Contains(c.CommitUid)
                        select  new Core.Persistence.Entities.Blob
                        {
                            BlobUid = b.BlobUid,
                            TreeUid = b.TreeUid,
                            //OriginUid = b.OriginUid,
                            Tree = new Tree
                            {
                                TreeUid = b.TreeUid,
                                CommitUid = b.Tree!.CommitUid
                            }
                        }).ToListAsync();

            var blobsPendent = new List<Core.Persistence.Entities.Blob>();

            var commits = await commitsToPull.ToListAsync();            
            
            await responseStream.WriteAsync(new pull_response() { Message = "+" });

            while (await requestStream.MoveNext())
            {
                var blob = requestStream.Current.Blob;            

                string originUid = blob.OriginUid;
                byte[] keyBytes = blob.Key.ToArray();
                byte[] recordBytes = blob.Record.ToArray();                

                if (!blobs.Any(b => b.Tree!.CommitUid == blob.Tree.CommitUid && b.TreeUid == blob.TreeUid && b.BlobUid == blob.BlobUid))
                {
                    blobs.Add(new Core.Persistence.Entities.Blob
                    {
                        BlobUid = blob.BlobUid,
                        TreeUid = blob.TreeUid,
                        //OriginUid = originUid
                    });

                    var newBlob = new Core.Persistence.Entities.Blob
                    {
                        BlobUid = blob.BlobUid,
                        TreeUid = blob.TreeUid,
                        //OriginUid = originUid,
                        //Key = keyBytes,
                        Content = recordBytes,
                        //Mode = 0,
                        //IsSent = true,
                        Tree = new Tree
                        {
                            TreeUid = blob.TreeUid,
                            CommitUid = blob.Tree.CommitUid,
                            Commit = new Commit
                            {
                                CommitUid = blob.Tree.Commit.CommitUid,
                                RepositoryUid = blob.Tree.Commit.RepositoryUid,
                                UserUid = blob.Tree.Commit.UserUid,
                                DateCreated = blob.Tree.Commit.Date.ToDateTime().ToLocalTime(),
                                Message = blob.Tree.Commit.Message,
                                MainTreeUid = blob.Tree.Commit.MainTreeUid,
                                ParentCommitUid = blob.Tree.Commit.ParentCommitUid                                
                            }
                        }
                    };

                    blobsPendent.Add(newBlob);
                }
                await responseStream.WriteAsync(new pull_response() { Message = "+" });
            }

            var commitRecords = from b in blobsPendent
                                group b.Tree by b.Tree!.Commit into gt
                                select gt.Key;

            var treesRecords = from b in blobsPendent
                               group b.Tree by b.Tree! into gt
                               select new Tree
                               {
                                   TreeUid = gt.Key.TreeUid,
                                   CommitUid = gt.Key.CommitUid
                               };

            _repositoryDbContext.Blobs!.AddRange(blobs);
            _repositoryDbContext.Commits!.AddRange(commitRecords);
            _repositoryDbContext.Trees!.AddRange(treesRecords);

            if (0 < await _repositoryDbContext.SaveEntitiesAsync())
            {
                var repository = _repositoryDbContext.Repositories.Where(r => r.RepositoryUid == repositoryUid)
                    .SingleOrDefault();
                if (repository == null)
                    throw new ArgumentException($"Repository {repositoryUid} not found!");
                else
                {
                    var pull = new Pull
                    {
                        PullUid = Guid.NewGuid().ToString(),
                        RepositoryUid = repositoryUid,
                        Date = DateTime.Now,
                        UserUid = userUid,
                        BrandName = repository!.CurrentBranchUid
                    };
                    _repositoryDbContext.Pulls!.Add(pull);

                    await _repositoryDbContext.SaveEntitiesAsync();
                }
            }
        }

        public override Task<branch_response> branch(branch_request request, ServerCallContext context)
        {
            return base.branch(request, context);
        }
        public override Task<checkout_response> checkout(checkout_request request, ServerCallContext context)
        {
            return base.checkout(request, context);
        }
        public override Task merge(IAsyncStreamReader<merge_request> requestStream, IServerStreamWriter<merge_response> responseStream, ServerCallContext context)
        {
            return base.merge(requestStream, responseStream, context);
        }
        public override Task status(IAsyncStreamReader<status_request> requestStream, IServerStreamWriter<status_response> responseStream, ServerCallContext context)
        {
            return base.status(requestStream, responseStream, context);
        }
    }
}