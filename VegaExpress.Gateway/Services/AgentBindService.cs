using Google.Api;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

using Polly;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using VegaExpress.Agent.Generated;
using VegaExpress.Gateway.Enums;

namespace VegaExpress.Worker.Services
{
    internal class AgentBindService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        protected CancellationTokenSource cancellationTokenSource { get; private set; }
        private readonly IAsyncPolicy policy;
        private readonly IConfiguration configuration;
        private readonly ILogger<AgentBindService> logger;        
        private Timer? timer;
        private AsyncDuplexStreamingCall<session, session>? call;

        public AgentBindService(IServiceProvider serviceProvider, ILogger<AgentBindService> logger, IConfiguration configuration, IAsyncPolicy policy)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
            this.policy = policy;

            cancellationTokenSource = new CancellationTokenSource();            
        }        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTokenSource.Token).Token;

            // Obtener el ensamblado actual
            var assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var assemblyName = assembly.GetName();

            // Obtener el proceso actual
            Process currentProcess = Process.GetCurrentProcess();
            int processId = currentProcess.Id;
            string processName = currentProcess.ProcessName;

            string serviceUid = attribute.Value;
            string serviceName = assemblyName.Name!;
            string serviceVersion = assemblyName.Version!.ToString();
            string serviceAddress = configuration["service-address"]!;
            string serviceAgentUid = configuration["service-agent-uid"]!;
            string serviceAgentAddress = configuration["service-agent-address"]!;            

            logger.LogInformation($"Connecting to {serviceAgentAddress}.");

            DateTime localDateTime = DateTime.Now;
            DateTime utcDateTime = localDateTime.ToUniversalTime();

            var endpointsStr = configuration["endpoints"];
            var endpointRoutes = GetEndpoints(endpointsStr!);

            var service = new gateway_routing_service
            {                
                ServiceUID = serviceUid,
                ServiceName = serviceName,
                ServiceVersion = serviceVersion,
                ServiceState = service_state.SsStarted,
                ServiceAddress = serviceAddress,
                ServiceAgentUID = serviceAgentUid,
                ProcessName = processName,
                ProcessID = processId,
                LatestSession = Timestamp.FromDateTime(utcDateTime),
                Server = GetComputerInfo(),                
            };
            service.Endpoints.AddRange(endpointRoutes);

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit!;

            await policy.ExecuteAsync(async (cancellationToken) =>
            {
                Thread.Sleep(100);                

                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var httpClient = new HttpClient(handler);
                var channel = GrpcChannel.ForAddress(serviceAgentAddress, new GrpcChannelOptions { HttpClient = httpClient });

                var client = new Bind2Service.Bind2ServiceClient(channel);                

                var headers = new Metadata { 
                    { "service-uid", serviceUid }, 
                    { "service-pid", processId.ToString() },
                    { "service-address", serviceAddress },
                    { "service-agent-uid", serviceAgentUid },
                    { "service-type", ServiceType.Worker.ToString() }
                };

                logger.LogInformation($"My service-uid is {serviceUid}, PID: {processId}.");

                using (call = client.Connect(headers: headers, cancellationToken: cancellationToken))
                {
                    headers = await call.ResponseHeadersAsync;                    

                    logger.LogInformation($"Inicialized in: {channel.Target}.");                                                     

                    var session = new session
                    {
                        Service = new service
                        { 
                            EventType = service_event_type.SetBeginningStream,                            
                            GatewayRoutingService = service
                        }
                    };                    
                    
                    await call.RequestStream.WriteAsync(session);

                    bool breakLoop = false;
                    while (await call.ResponseStream.MoveNext(cancellationToken))
                    {
                        session responseSession = call.ResponseStream.Current;

                        switch (responseSession.Service.EventType)
                        {                    
                            case service_event_type.SetStreamStarted:
                                {
                                    var serviceInstance = responseSession.Service.GatewayRoutingService;
                                    logger.LogInformation($"Service-uid: {serviceInstance.ServiceUID}, Started broadcast.");
                                }
                                break;
                            case service_event_type.SetEndingStream:
                                {
                                    service.ServiceState = service_state.SsStopped;
                                    service.ServiceIsBusy = false;

                                    session = new session
                                    {
                                        Service = new service
                                        { 
                                            EventType = service_event_type.SetStreamEnded,                                            
                                            GatewayRoutingService = service
                                        }                                        
                                    };

                                    await call.RequestStream!.WriteAsync(session);

                                    Thread.Sleep(100);
                                    breakLoop = true;                                    
                                }
                                break;
                            case service_event_type.SetFinishingStream:
                                {
                                    service.ServiceState = service_state.SsStopped;
                                    service.ServiceIsBusy = false;

                                    session = new session
                                    {
                                        Service = new service
                                        { 
                                            EventType = service_event_type.SetFinishedStream,                                            
                                            GatewayRoutingService = service
                                        }
                                    };

                                    await call.RequestStream!.WriteAsync(session);                                    

                                    Thread.Sleep(100);
                                    breakLoop = true;
                                }
                                break;
                        }
                        if (breakLoop) break;
                    }

                }
                
                await channel.ShutdownAsync();
                await StopAsync(cancellationToken);

            }, stoppingToken);

            async void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                try
                {
                    StatusCode statusCode = call?.GetStatus().StatusCode ?? StatusCode.NotFound;
                    if (statusCode == StatusCode.NotFound || statusCode == StatusCode.Unavailable) return;
                    
                    var headers = await call!.ResponseHeadersAsync;
                    service.ServiceState = service_state.SsStopped;
                    service.ServiceIsBusy = false;

                    var session = new session
                    {
                        Service = new service
                        {
                            EventType = service_event_type.SetStreamEnded,
                            GatewayRoutingService = service
                        }
                    };                    

                    await call.RequestStream!.WriteAsync(session);
                }
                catch (ObjectDisposedException) { }
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            call?.Dispose();

            var hostApplicationLifetime = this.serviceProvider.GetService<IHostApplicationLifetime>();
            hostApplicationLifetime?.StopApplication();

            await base.StopAsync(stoppingToken);
        }

        IEnumerable<route_endpoint> GetEndpoints(string endpointStr)
        {
            string pattern = "(GET|POST|PUT|DELETE|PATCH)([^GET|POST|PUT|DELETE|PATCH]*)";

            MatchCollection matches = Regex.Matches(endpointStr, pattern);

            List<route_endpoint> result = new List<route_endpoint>();

            foreach (Match match in matches)
            {
                result.Add(new route_endpoint
                {
                    Method = match.Groups[1].Value,
                    Pattern = match.Groups[2].Value,
                });                
            }
            return result;
        }

        private server GetComputerInfo()
        {
            // Hostname
            string hostName = Environment.MachineName;            

            // Username
            string userName = Environment.UserName;            

            // OS
            string os = RuntimeInformation.OSDescription;

            // OS Architecture
            string osArquitecture = RuntimeInformation.OSArchitecture.ToString();

            // Processor(s)
            int processor = Environment.ProcessorCount;

            // Process Architecture
            string processArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

            // RAM Capacity
            var memoryInfo = GC.GetGCMemoryInfo();
            string RAM = Math.Round((double)memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024 / 1024, 2) + " GB";

            // Network Information
            server computer = new server
            {
                HostName = hostName,
                UserName = userName,
                OS = os,
                OSArquitecture = osArquitecture,
                Processors = processor,
                ProcessArquitecture = processArchitecture,
                RAM = RAM,
            };            

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProps = nic.GetIPProperties();
                    string ipv4 = null!, ipv6 = null!;
                    foreach (var ip in ipProps.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipv4 = ip.Address.ToString();                            
                        }
                        else if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            ipv6 = ip.Address.ToString();                            
                        }
                    }

                    computer.NetworkAddresses.Add(new network_address
                    {
                        Name = nic.Name,
                        MAC = nic.GetPhysicalAddress().ToString(),
                        IPv4 = ipv4,
                        IPv6 = ipv6,
                        Speed = nic.Speed
                    });
                }
            }            

            return computer;
        }        
    }
}
