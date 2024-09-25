using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

using Polly;

using VegaExpress.Worker.Storage.Generated;

namespace VegaExpress.Worker.Services
{
    public class StorageService : BackgroundService
    {
        protected CancellationTokenSource cancellationTokenSource { get; private set; }
        private readonly ILogger<StorageService> logger;
        private readonly IConfiguration configuration;
        private readonly IAsyncPolicy policy;
        public StorageService(ILogger<StorageService> logger, IConfiguration configuration, IAsyncPolicy policy)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.policy = policy;

            cancellationTokenSource = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTokenSource.Token).Token;

            string serviceAddress = configuration["service-address"]!;
            string serviceAgentUid = configuration["service-agent-uid"]!;
            string serviceAgentAddress = configuration["service-agent-address"]!;            

            await policy.ExecuteAsync(async (cancellationToken) =>
            {
                //var handler = new HttpClientHandler();
                //handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                //var httpClient = new HttpClient(handler);
                //var channel = GrpcChannel.ForAddress(serviceAgentAddress, new GrpcChannelOptions { HttpClient = httpClient });

                //while (cancellationToken.IsCancellationRequested)
                //{
                //    var client = new storage_db_service.storage_db_serviceClient(channel);

                //    var pullHeaders = new Metadata { };
                //    var pullRequest = new pull_request
                //    {
                //        Branch = "",
                //        Remote = ""
                //    };

                //    using (var call = client.pull(pullRequest, pullHeaders, cancellationToken: cancellationToken))
                //    {
                //        while (await call.ResponseStream.MoveNext())
                //        {
                //            var pullResponse = call.ResponseStream.Current;
                //            logger.LogInformation($"Pull response: {pullResponse}");
                //        }
                //    }

                //    var pushHeaders = new Metadata
                //    {
                //            { "branch", "" },   // branch to push changes to
                //            { "remote", "" },   // remote repository URL
                //    };
                //    var pushRequests = new push_request[]
                //    {
                //        new push_request
                //        {
                //            Changes = new changes {
                //                OriginUid = "",
                //                // Key
                //                // record
                //            }
                //        }
                //    };

                //    using (var call = client.push(pushHeaders, cancellationToken: cancellationToken))
                //    {
                //        foreach (var pushRequest in pushRequests)
                //        {
                //            await call.RequestStream.WriteAsync(pushRequest);
                //        }

                //        await call.RequestStream.CompleteAsync();

                //        while (await call.ResponseStream.MoveNext())
                //        {
                //            var pushResponse = call.ResponseStream.Current;
                //            logger.LogInformation($"Push response: {pushResponse}");
                //        }
                //    }

                //    await Task.Delay(TimeSpan.FromSeconds(3));
                //}

                //await channel.ShutdownAsync();

            }, stoppingToken);
        }
    }
}
