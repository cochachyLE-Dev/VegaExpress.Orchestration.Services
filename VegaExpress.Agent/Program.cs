using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using VegaExpress.Agent.Services;
using VegaExpress.Agent.Services.Extentions;
using VegaExpress.Worker.Services;
using VegaExpress.Worker.Storage.Generated;

IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure((IApplicationBuilder app) =>
                    {
                        app.UseRouting();
                        app.UseAgentService();
                        app.UseEndpoints(endpoints =>
                        {                            
                            endpoints.MapAgentService();
                            endpoints.MapGrpcService<StorageService>();
                        });
                    });
                    webBuilder.ConfigureKestrel((context, serverOptions) => serverOptions.AddAgentkestrel(context));
                    webBuilder.ConfigureServices((context, services) => services.AddAgentService());
                });

host.Build().Run();