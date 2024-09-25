using Google.Api;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.AspNetCore.Mvc.ApplicationParts;

using Polly;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;

using VegaExpress.Agent.Generated;
using VegaExpress.Worker.Enums;
using VegaExpress.Worker.Utilities;

namespace VegaExpress.Worker.Services
{
    internal class AgentBindService : BackgroundBase
    {
        protected CancellationTokenSource cancellationTokenSource { get; private set; }
        private readonly ILogger<AgentBindService> logger;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;        
        private readonly IAsyncPolicy policy;       
        private readonly NetworkMonitor networkMonitor;
        private AsyncDuplexStreamingCall<session, session>? call;

        public AgentBindService(ILogger<AgentBindService> logger, IConfiguration configuration, IServiceProvider serviceProvider, IAsyncPolicy policy)
        {
            this.serviceProvider = serviceProvider;            
            this.logger = logger;
            this.configuration = configuration;
            this.policy = policy;

            this.networkMonitor = new NetworkMonitor();
            cancellationTokenSource = new CancellationTokenSource();            
        }        
        protected override async Task ExecuteWorkAsync(CancellationToken stoppingToken)
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

            var service = new worker_service
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
                Server = GetComputerInfo()
            };

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
                string serviceTypeStr = headers.Get("service-type")!.Value;

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
                            WorkerService = service
                        }
                    };
                    
                    await call.RequestStream.WriteAsync(session);

                    networkMonitor.InternetAvaliableChanged += async (location) =>
                    {

                        var networkMonitor2 = new network_monitor();
                        networkMonitor2.IsInternetAvaliable = networkMonitor.IsInternetAvaliable!.Value;

                        var isInternetAvaliable = networkMonitor.IsInternetAvaliable;
                        if (location != null)
                        {
                            networkMonitor2.Details.Add("ip", location.Ip);
                            networkMonitor2.Details.Add("city", location.City);
                            networkMonitor2.Details.Add("region", location.Region ?? "");
                            networkMonitor2.Details.Add("country-name", location.CountryName);
                        }

                        var session = new session
                        {
                            Service = new service
                            {
                                EventType = service_event_type.SetConnectionEvent,
                                WorkerService = service
                            },
                            Message = new msg
                            {
                                NetworkMonitor = networkMonitor2
                            }
                        };

                        await call.RequestStream.WriteAsync(session);

                    };

                    bool breakLoop = false;
                    while (await call.ResponseStream.MoveNext(cancellationToken))
                    {
                        session responseSession = call.ResponseStream.Current;

                        switch (responseSession.Service.EventType)
                        {                            
                            case service_event_type.SetStreamStarted:
                                {
                                    var serviceInstance = responseSession.Service.WorkerService;
                                    logger.LogInformation($"Service-uid: {serviceInstance.ServiceUID}, Started broadcast.");
                                }
                                break;
                            case service_event_type.SetEndingStream:
                                {
                                    service.ServiceState = service_state.SsStopped;
                                    service.ServiceIsBusy = false;                                    

                                    session = new session
                                    {
                                        Service = new service {
                                            EventType = service_event_type.SetStreamEnded,                                            
                                            WorkerService = service                                        
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
                                            WorkerService = service
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
                if (call == null || call.GetStatus().StatusCode == StatusCode.Unavailable) return;
                var headers = await call!.ResponseHeadersAsync;

                try
                {
                    service.ServiceState = service_state.SsStopped;
                    service.ServiceIsBusy = false;

                    var session = new session
                    {
                        Service = new service
                        {
                            EventType = service_event_type.SetStreamEnded,
                            WorkerService = service
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
