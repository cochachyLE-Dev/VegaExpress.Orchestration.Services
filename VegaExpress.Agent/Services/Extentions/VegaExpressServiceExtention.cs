using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using System.Net;
using System.Security.Cryptography.X509Certificates;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Data.Repository;
using VegaExpress.Agent.Services.Mapping;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.ViewModels.Dialog;

namespace VegaExpress.Agent.Services.Extentions
{
    public static class VegaExpressServiceExtention
    {
        public static void UseAgentService(this IApplicationBuilder builder) { }
        public static void MapAgentService(this IEndpointRouteBuilder builder)
        {
            builder.MapGrpcService<BindService>();
        }
        public static void AddAgentkestrel(this KestrelServerOptions serverOptions, WebHostBuilderContext context)
        {
            var serviceAddress = context.Configuration["service-address"];
            var uri = new Uri(serviceAddress!);

            serverOptions.Listen(IPAddress.Parse(uri.Host), uri.Port,
                listenOptions =>
                {
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        var localhostCert = CertificateLoader.LoadFromStoreCert(
                            "localhost", "My", StoreLocation.CurrentUser,
                            allowInvalid: true);

                        var certs = new Dictionary<string, X509Certificate2>(
                            StringComparer.OrdinalIgnoreCase);

                        certs["localhost"] = localhostCert;

                        httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
                        {
                            if (name != null && certs.TryGetValue(name, out var cert))
                            {
                                return cert;
                            }

                            return localhostCert;
                        };
                    });
                });
        }
        public static void AddAgentService(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new BindServiceProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddSingleton<IServiceProvider>(provider => provider);
            services.AddSingleton<LocalMessaging<ServiceManagement.ScheduledTask>>();
            services.AddSingleton<LocalMessaging<ProcessThreadModel>>();
            services.AddSingleton<LocalMessaging<IServiceInstance>>();
            services.AddSingleton<LocalMessaging<WorkerServiceModel>>();
            services.AddSingleton<LocalMessaging<GatewayRoutingServiceModel>>();
            services.AddSingleton<LocalMessaging<LoadBalancerServiceModel>>();
            services.AddSingleton<LocalMessaging<MessageQueueServiceModel>>();
            services.AddSingleton<LocalMessaging<TaskSchedulerServiceModel>>();
            services.AddSingleton<LocalMessaging<ServerModel>>();
            services.AddSingleton<LocalMessaging<MessageLogModel>>();

            services.AddSingleton<IWorkerServiceRepository, WorkerServiceRepository>();
            services.AddSingleton<IWorkerServiceInstanceRepository, WorkerServiceInstanceRepository>();
            services.AddSingleton<IGatewayRoutingServiceRepository, GatewayRoutingServiceRepository>();

            services.AddTransient<IRegisterServiceViewModel, RegisterServiceViewModel>();
            services.AddTransient<IRegisterGatewayServiceViewModel, RegisterGatewayServiceViewModel>();

            services.AddSingleton<BindServiceSubscriptor>();
            services.AddGrpc();

            services.AddRouteServices();
            services.AddHostedService<ServiceManagement>();
        }
    }
}
