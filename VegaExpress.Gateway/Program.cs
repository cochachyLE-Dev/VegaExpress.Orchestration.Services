using Microsoft.AspNetCore.Server.Kestrel.Https;

using Polly;

using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

using VegaExpress.Gateway.Extentions;
using VegaExpress.Gateway.Middlewares;
using VegaExpress.Worker.Services;


IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure((WebHostBuilderContext context, IApplicationBuilder app) =>
                    {
                        var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("VegaExpress.Gateway");

                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseMiddleware<RedirectMiddleware>();
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.Use(async (context, next) =>
                        {                                                        
                            logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");                                
                            await next.Invoke();
                            logger.LogInformation($"Response: {context.Response.StatusCode}");                          
                        });

                        var endpointsStr = context.Configuration["endpoints"];
                        var endpointRoutes = GetEndpoints(endpointsStr!);

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/home", async context =>
                            {
                                await context.Response.WriteAsync("Gateway Routing API!");
                            });
                          
                            //foreach (var route in endpointRoutes)
                            //{ 
                            //    endpoints.MapRoute(route.method, route.pattern, "/home");
                            //}
                        });

                        (string method, string pattern)[] GetEndpoints(string endpointStr)
                        {
                            string pattern = "(GET|POST|PUT|DELETE|PATCH)([^GET|POST|PUT|DELETE|PATCH]*)";

                            MatchCollection matches = Regex.Matches(endpointStr, pattern);

                            List<(string, string)> result = new List<(string, string)>();

                            foreach (Match match in matches)
                            {
                                result.Add((match.Groups[1].Value, match.Groups[2].Value));
                            }
                            return result.ToArray();
                        }
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
                        services.AddLogging();
                        services.AddLogging();
                        services.AddRouting();

                        services.AddSingleton<IServiceProvider>(provider => provider);
                        services.AddSingleton<Polly.IAsyncPolicy>(sp =>
                        {
                            return Polly.Policy.Handle<Exception>().WaitAndRetryAsync(3, retryAttempt =>
                                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                        });                        

                        // Add services to the container.
                        services.AddAuthentication("Bearer")
                                    .AddJwtBearer("Bearer", options =>
                                    {
                                        options.Authority = "https://auth-server.com";
                                        options.Audience = "api-gateway";
                                    });

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("ApiScope", policy =>
                            {
                                policy.RequireAuthenticatedUser();
                                policy.RequireClaim("scope", "api");
                            });
                        });

                        services.AddGrpc();                        
                        //services.AddHostedService<AgentBindService>();                        
                    });
                });

host.Build().Run();

//var builder = WebApplication.CreateBuilder(args);