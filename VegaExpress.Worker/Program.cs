using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using Polly;

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using VegaExpress.Worker.Services;
using VegaExpress.Worker.Services.Events;

IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {                        
                    webBuilder.Configure((IApplicationBuilder app) =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<ProcessThreadService>();
                            endpoints.MapGet("/home", async context =>
                            {
                                await context.Response.WriteAsync("Hello work!");
                            });
                        });
                    });
                    webBuilder.ConfigureKestrel((context, serverOptions) =>
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
                    });
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<IServiceProvider>(provider => provider);
                        services.AddGrpc();
                        // Register Polly policy
                        services.AddSingleton<Polly.IAsyncPolicy>(sp =>
                        {
                            return Polly.Policy.Handle<Exception>().WaitAndRetryAsync(3, retryAttempt =>
                                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                        });
                        //services.AddSingleton<ResourceUsageMonitor>();
                        //services.AddHostedService<ResourceUsageService>();
                        services.AddHostedService<AgentBindService>();
                        //services.AddHostedService<WorkerService>();
                    });
                });

host.Build().Run();