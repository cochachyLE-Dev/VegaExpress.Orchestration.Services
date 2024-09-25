using Akavache;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;

using VegaExpress.Worker.Client;
using VegaExpress.Worker.Client.Extentions;
using VegaExpress.Worker.Core;
using VegaExpress.Worker.Core.Services;

//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureAppConfiguration((hostingContext, config) =>
//    {
//        config.AddEnvironmentVariables();
//        config.AddCommandLine(args);
//    })
//    .ConfigureServices((hostContext,services) =>
//    {
//        services.AddControllers();

//        BlobCache.ApplicationName = "Client";
//        services.AddSingleton(typeof(IBlobCache), BlobCache.LocalMachine);
//        services.AddDbContext<RepositoryDbContext>(o => o.UseNpgsql(hostContext.Configuration.GetConnectionString("StorageDB")));

//        services.AddHostedService<Worker>();
//    })
//    .Build();

//host.Run();

public partial class Program
{
    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += async (s, e) =>
        {
            await BlobCache.LocalMachine.InvalidateAll();
        };

        var isService = !(Debugger.IsAttached || args.Contains("--console"));

        var builder = CreateHostBuilder(args);

        if (isService)
        {
            var pathToContentRoot = AppContext.BaseDirectory;
            await builder.UseContentRoot(pathToContentRoot)
                .Build()
                .RunAsync();

        }
        else
        {
            await builder.Build().RunAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)                      
            .ConfigureWebHostDefaults(webBuilder => {

                // Configurar el servidor web (kestrel) desde la configuraciòn                
                webBuilder.ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                });

                webBuilder.ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    services.AddDbContext(config);
                    services.AddScopedServices();
                    services.AddSingletonServices();
                    services.AddTransientServices();
                    services.AddSwaggerOpenAPI();
                    services.AddIdentityService(config);

                    // Agregar el servicio en segundo plano
                    services.AddHostedService<Worker>();

                    // Configurar servicios de controladores para la API REST
                    services.AddControllers();
                    services.AddEndpointsApiExplorer(); 
                });                

                webBuilder.Configure((context, app) =>
                {
                    var config = context.Configuration;                    

                    app.UseCors(options => options.WithOrigins("*").AllowAnyHeader().AllowAnyMethod());

                    app.UseRouting();

                    app.UseAuthentication();
                    app.UseAuthorization();
                                     
                    app.ConfigureSwagger();

                    app.UseHttpsRedirection();

                    app.UseAuthorization();

                    app.UseHttpsRedirection();

                    app.UseAuthorization();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

                webBuilder.ConfigureKestrel((context, serverOptions) =>
                {
                    var serviceAddress = context.Configuration["VegaExpress.Worker:Address"];
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
            });
    }
}