using Google.Protobuf;
using Grpc.Core;
using VegaExpress.Worker.Storage.Generated;

namespace VegaExpress.Worker.Services
{
    public class StorageService : storage_db_service.storage_db_serviceBase
    {
        private readonly ILogger<StorageService> logger;
        public StorageService(ILogger<StorageService> logger)
        {
            this.logger = logger;
        }

        public override async Task<init_response> init(init_request request, ServerCallContext context)
        {
            var response = new init_response();
            return await Task.FromResult(response);
        }
        public override async Task clone(clone_request request, IServerStreamWriter<clone_response> responseStream, ServerCallContext context)
        {
            string url = request.Url;

            byte[] data = { /* Repository data bytes */ };
            ByteString byteString = ByteString.CopyFrom(data);
            await responseStream.WriteAsync(new clone_response { Data = byteString });

            //await responseStream.WriteCompletedAsync();
        }
        public override Task<add_response> add(add_request request, ServerCallContext context)
        {
            return base.add(request, context);
        }
        public override Task<commit_response> commit(commit_request request, ServerCallContext context)
        {
            return base.commit(request, context);
        }
        public override Task push(IAsyncStreamReader<push_request> requestStream, IServerStreamWriter<push_response> responseStream, ServerCallContext context)
        {
            return base.push(requestStream, responseStream, context);
        }
        public override Task pull(IAsyncStreamReader<pull_request> requestStream, IServerStreamWriter<pull_response> responseStream, ServerCallContext context)
        {
            return base.pull(requestStream, responseStream, context);
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
