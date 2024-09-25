using AutoMapper;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Net.Client;

using System.Collections.Concurrent;

using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Generated;
using VegaExpress.Agent.Shared;
using VegaExpress.Worker.Extentions;

using static VegaExpress.Agent.Generated.service;

namespace VegaExpress.Agent.Services
{
    public class BindServiceSubscriptor
    {
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<string, IServerStreamWriter<session>> _sessionServerStreamWriter = new ConcurrentDictionary<string, IServerStreamWriter<session>>();
        private readonly ConcurrentDictionary<string, AsyncServerStreamingCall<ProcessThread>> _monitorStreamingCalls = new ConcurrentDictionary<string, AsyncServerStreamingCall<ProcessThread>>();
        
        public readonly LocalMessaging<ServiceManagement.ScheduledTask> MessageQueueSMScheduledTask;
        public readonly LocalMessaging<ProcessThreadModel> MessageQueueProcessThread;
        public readonly LocalMessaging<IServiceInstance> MessageQueueServiceInstance;
        public readonly LocalMessaging<GatewayRoutingServiceModel> MessageQueueGatewayRoutingServiceInstance;
        public BindServiceSubscriptor(IMapper mapper, LocalMessaging<ServiceManagement.ScheduledTask> messageQueueSMScheduledTask, LocalMessaging<IServiceInstance> messageQueueServiceInstance, LocalMessaging<GatewayRoutingServiceModel> messageQueueGatewayRoutingServiceInstance, LocalMessaging<ProcessThreadModel> messageQueueProcessThread) {
            _mapper = mapper;
            MessageQueueSMScheduledTask = messageQueueSMScheduledTask;
            MessageQueueServiceInstance = messageQueueServiceInstance;
            MessageQueueGatewayRoutingServiceInstance = messageQueueGatewayRoutingServiceInstance;
            MessageQueueProcessThread = messageQueueProcessThread;
        }
    
        public void AddOrUpdateServiceInstance(worker_service serviceInstance)
        {
            WorkerServiceInstanceModel serviceInstanceModel = _mapper.Map<WorkerServiceInstanceModel>(serviceInstance);
            MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstanceModel, ServiceManagement.ActionType.AddOrUpdateServiceInstance));
        }
        public void AddOrUpdateServiceInstance(gateway_routing_service serviceInstance)
        {
            GatewayRoutingServiceModel serviceInstanceModel = _mapper.Map<GatewayRoutingServiceModel>(serviceInstance);
            MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstanceModel, ServiceManagement.ActionType.AddOrUpdateServiceInstance));
        }

        public void DisposeAllServiceInstances()
        {
            foreach (var client in _sessionServerStreamWriter)
            {
                var session = new session
                {
                    Service = new service
                    {
                        EventType = service_event_type.SetStoppingService,                        
                        WorkerService = new worker_service { ServiceAddress = client.Key }                        
                    }
                };
                SessionServerStreamWriter(client.Key!, session);
            }
        }
        public void StopServiceInstance(worker_service serviceInstance)
        {
            serviceInstance.ServiceState = service_state.SsStopped;
            serviceInstance.LatestSession = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());

            WorkerServiceInstanceModel serviceInstanceModel = _mapper.Map<WorkerServiceInstanceModel>(serviceInstance);
            MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstanceModel, ServiceManagement.ActionType.StopServiceInstance));
        }
        public void StopServiceInstance(gateway_routing_service serviceInstance)
        {
            serviceInstance.ServiceState = service_state.SsStopped;
            serviceInstance.LatestSession = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());

            GatewayRoutingServiceModel serviceInstanceModel = _mapper.Map<GatewayRoutingServiceModel>(serviceInstance);
            MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstanceModel, ServiceManagement.ActionType.StopServiceInstance));
        }

        public void AddOrUpdateSession(string serviceInstanceAddress, IServerStreamWriter<session> serverStreamWriter)
        {
            _sessionServerStreamWriter.AddOrUpdate(serviceInstanceAddress, serverStreamWriter, (k, v) => serverStreamWriter);
        }

        public void AddOrUpdateMonitor(string serviceInstanceAddress, AsyncServerStreamingCall<ProcessThread> asyncServerStreamingCall)
        {
            _monitorStreamingCalls.AddOrUpdate(serviceInstanceAddress, asyncServerStreamingCall, (k, v) => asyncServerStreamingCall);
        }

        public async void SessionServerStreamWriter(string serviceInstanceAddress, session session)
        {
            try
            {
                if (_sessionServerStreamWriter.TryGetValue(serviceInstanceAddress, out IServerStreamWriter<session>? sessionServerStreamWriter))
                {
                    await sessionServerStreamWriter.WriteAsync(session);
                }
            }
            catch (ObjectDisposedException) { }
        }
        public void StopServiceInstanceMonitor(string serviceInstanceAddress)
        {
            if (_monitorStreamingCalls.TryGetValue(serviceInstanceAddress, out AsyncServerStreamingCall<ProcessThread>? monitorStreamingCalls))
            {
                monitorStreamingCalls.Dispose();
            }
        }
        internal async void StartServiceInstanceMonitor(string serviceInstanceAddress)
        {
            try
            {                
                if (_monitorStreamingCalls.TryGetValue(serviceInstanceAddress, out AsyncServerStreamingCall<ProcessThread>? monitorStreamingCall))
                {
                    monitorStreamingCall.Dispose();
                }

                var httpHandler = new HttpClientHandler();
                // WARNING: Use this only for testing, do not disable SSL verification in production environments
                httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var channel = GrpcChannel.ForAddress(serviceInstanceAddress!, new GrpcChannelOptions { HttpHandler = httpHandler });
                var client = new ProcessThreadService.ProcessThreadServiceClient(channel);

                var request = new ProcessThreadRequest { };
                monitorStreamingCall = client.StreamProcessThreads(request);

                _monitorStreamingCalls.AddOrUpdate(serviceInstanceAddress!, monitorStreamingCall, (key, oldValue) => monitorStreamingCall);

                await foreach (var processThread in monitorStreamingCall.ResponseStream.ReadAllAsync())
                {
                    var processThreadModel = new ProcessThreadModel();
                    processThreadModel.TaskID = processThread.Id;
                    processThreadModel.BasePriority = processThread.BasePriority;
                    processThreadModel.CurrentPriority = processThread.CurrentPriority;
                    processThreadModel.PriorityLevel = processThreadModel.ConvertEnumTo<Generated.ThreadPriorityLevel, System.Diagnostics.ThreadPriorityLevel>(processThread.PriorityLevel);
                    processThreadModel.StartTime = processThread.StartTime.ToDateTime();
                    processThreadModel.TotalProcessorTime = processThread.TotalProcessorTime.ToTimeSpan();
                    processThreadModel.UserProcessorTime = processThread.UserProcessorTime.ToTimeSpan();
                    processThreadModel.PrivilegedProcessorTime = processThread.PrivilegedProcessorTime.ToTimeSpan();
                    processThreadModel.ThreadState = processThreadModel.ConvertEnumTo<Generated.ThreadState, System.Diagnostics.ThreadState>(processThread.ThreadState);
                    processThreadModel.WaitReason = processThreadModel.ConvertEnumTo<Generated.ThreadWaitReason, System.Diagnostics.ThreadWaitReason>(processThread.WaitReason);

                    MessageQueueProcessThread.SendMessage(processThreadModel);
                }
            }
            catch
            {
            }
        }
    }
    public class BindService : VegaExpress.Agent.Generated.Bind2Service.Bind2ServiceBase
    {
        private readonly ILogger<BindService> _logger;        
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly BindServiceSubscriptor _subscriptor;

        public BindService(ILogger<BindService> logger, IMapper mapper, IConfiguration configuration, BindServiceSubscriptor bindServiceSubscriptor)
        {            
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _subscriptor = bindServiceSubscriptor;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit!);
        }        
        public override async Task Connect(IAsyncStreamReader<session> requestStream, IServerStreamWriter<session> responseStream, ServerCallContext context)
        {
            try
            {
                var connectionInfo = context.GetConnectionInfo();
                _logger.LogInformation($"Inicialized in IP: {connectionInfo.RemoteIpAddress}, Port: {connectionInfo.RemotePort}.");

                var service_uid = _configuration.GetValue<string>("VegaExpress:Service:UID")!;

                var headers = context.ResponseTrailers;
                headers.Add("agent-uid", service_uid);

                _logger.LogInformation($"My service-uid is: {service_uid}");

                await context.WriteResponseHeadersAsync(headers);

                bool breakLoop = false;
                await foreach (var session in requestStream.ReadAllAsync())
                {
                    switch (session.Service.EventType)
                    {
                        case service_event_type.SetBeginningStream:
                            {
                                switch (session.Service.ServiceTypeCase)
                                {
                                    case ServiceTypeOneofCase.WorkerService:
                                        {
                                            _subscriptor.AddOrUpdateServiceInstance(session.Service.WorkerService);
                                            _subscriptor.AddOrUpdateSession(session.Service.WorkerService.ServiceAddress, responseStream);

                                            var response = new session()
                                            {
                                                Service = new service
                                                {
                                                    EventType = service_event_type.SetServiceStarted,                                                    
                                                    WorkerService = session.Service.WorkerService
                                                }
                                            };
                                            await responseStream.WriteAsync(response!);

                                            var serviceInstance = _mapper.Map<WorkerServiceInstanceModel>(session.Service.WorkerService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);

                                            _logger.LogInformation($"Connected worker service: {session.Service.WorkerService.ServiceName}, process: {session.Service.WorkerService.ProcessID}");
                                        }
                                        break;
                                    case ServiceTypeOneofCase.GatewayRoutingService:
                                        {
                                            _subscriptor.AddOrUpdateServiceInstance(session.Service.GatewayRoutingService);
                                            _subscriptor.AddOrUpdateSession(session.Service.GatewayRoutingService.ServiceAddress, responseStream);

                                            var response = new session()
                                            {
                                                Service = new service {                                                 
                                                    EventType = service_event_type.SetServiceStarted,                                                    
                                                    GatewayRoutingService = session.Service.GatewayRoutingService
                                                }
                                            };
                                            await responseStream.WriteAsync(response!);

                                            var serviceInstance = _mapper.Map<GatewayRoutingServiceModel>(session.Service.GatewayRoutingService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);

                                            _logger.LogInformation($"Connected gateway service: {session.Service.GatewayRoutingService.ServiceName}, process: {session.Service.GatewayRoutingService.ProcessID}");
                                        }
                                        break;
                                } 

                            }
                            break;
                        case service_event_type.SetStreamEnded:
                            {
                                switch (session.Service.ServiceTypeCase)
                                {
                                    case ServiceTypeOneofCase.WorkerService:
                                        {
                                            session.Service.WorkerService.ServiceState = service_state.SsStopped;

                                            var serviceInstance = _mapper.Map<WorkerServiceInstanceModel>(session.Service.WorkerService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);

                                            _logger.LogInformation($"StreamEnded service: {session.Service.WorkerService.ProcessID}");
                                            breakLoop = true;
                                        }
                                        break;
                                    case ServiceTypeOneofCase.GatewayRoutingService:
                                        {
                                            session.Service.GatewayRoutingService.ServiceState = service_state.SsStopped;

                                            var serviceInstance = _mapper.Map<GatewayRoutingServiceModel>(session.Service.GatewayRoutingService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);

                                            _logger.LogInformation($"StreamEnded service: {session.Service.GatewayRoutingService.ProcessID}");
                                            breakLoop = true;
                                        }
                                        break;
                                }
                            }
                            break;
                        case service_event_type.SetFinishedStream:
                            {
                                switch (session.Service.ServiceTypeCase)
                                {
                                    case ServiceTypeOneofCase.WorkerService:
                                        {
                                            session.Service.WorkerService.ServiceState = service_state.SsStopped;

                                            var serviceInstance = _mapper.Map<WorkerServiceInstanceModel>(session.Service.WorkerService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance, MessageAction.Remove);
                                            _logger.LogInformation($"FinishedStream service: {session.Service.WorkerService.ProcessID}");
                                            breakLoop = true;
                                        }
                                        break;
                                    case ServiceTypeOneofCase.GatewayRoutingService:
                                        {
                                            session.Service.GatewayRoutingService.ServiceState = service_state.SsStopped;

                                            var serviceInstance = _mapper.Map<GatewayRoutingServiceModel>(session.Service.GatewayRoutingService);
                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance, MessageAction.Remove);
                                            _logger.LogInformation($"FinishedStream service: {session.Service.GatewayRoutingService.ProcessID}");
                                            breakLoop = true;
                                        }
                                        break;
                                }
                            }
                            break;
                        case service_event_type.SetConnectionEvent: 
                            {
                                bool isInternetAvaliable = session.Message.NetworkMonitor.IsInternetAvaliable;
                                var details = session.Message.NetworkMonitor.Details;

                                var ipGeoLocation = new IpGeoLocationModel
                                {
                                    Ip = details.GetValueOrDefault("ip"),
                                    City = details.GetValueOrDefault("city"),
                                    Region = details.GetValueOrDefault("region"),
                                    CountryName = details.GetValueOrDefault("country-name")
                                };

                                switch (session.Service.ServiceTypeCase)
                                {
                                    case ServiceTypeOneofCase.WorkerService:
                                        {
                                            session.Service.WorkerService.ServiceState = service_state.SsStarted;

                                            var serviceInstance = _mapper.Map<WorkerServiceInstanceModel>(session.Service.WorkerService);
                                            serviceInstance.Server!.IpGeoLocation = ipGeoLocation;

                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);
                                            _logger.LogInformation($"Network connection Changed: {session.Service.WorkerService.ProcessID}");                                            
                                        }
                                        break;
                                    case ServiceTypeOneofCase.GatewayRoutingService:
                                        {
                                            session.Service.GatewayRoutingService.ServiceState = service_state.SsStarted;
                                            var serviceInstance = _mapper.Map<GatewayRoutingServiceModel>(session.Service.GatewayRoutingService);
                                            serviceInstance.Server!.IpGeoLocation = ipGeoLocation;

                                            _subscriptor.MessageQueueServiceInstance.SendMessage(serviceInstance);
                                            _logger.LogInformation($"Network connection Changed: {session.Service.GatewayRoutingService.ProcessID}");                                            
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    if (breakLoop) break;
                }
            }
            catch (IOException ex)
            {
                var headers = context.RequestHeaders;
                string serviceInstanceUid = headers.Get("service-uid")!.Value;
                string serviceInstancePid = headers.Get("service-pid")!.Value;
                string serviceInstanceAddress = headers.Get("service-address")!.Value;
                string serviceInstanceAgentUid = headers.Get("service-agent-uid")!.Value;
                string serviceTypeStr = headers.Get("service-type")!.Value;

                _logger.LogInformation($"La conexión ha sido interrumpida ({serviceInstancePid}): {ex.Message}");

                ServiceType serviceType = (ServiceType)System.Enum.Parse(typeof(ServiceType), serviceTypeStr);
                switch (serviceType)
                {
                    case ServiceType.Worker:
                        {
                            _subscriptor.StopServiceInstance(new worker_service()
                            {
                                ServiceUID = serviceInstanceUid,
                                ServiceAddress = serviceInstanceAddress,
                                ProcessID = int.Parse(serviceInstancePid),
                                ServiceAgentUID = serviceInstanceAgentUid,
                                ServiceState = service_state.SsStopped,
                                Server = new server()
                            });
                        }
                        break;
                    case ServiceType.GatewayRoute:
                        {
                            _subscriptor.StopServiceInstance(new gateway_routing_service()
                            {
                                ServiceUID = serviceInstanceUid,
                                ServiceAddress = serviceInstanceAddress,
                                ProcessID = int.Parse(serviceInstancePid),
                                ServiceAgentUID = serviceInstanceAgentUid,
                                ServiceState = service_state.SsStopped,
                                Server = new server()
                            });
                        }
                        break;
                }

                //context.Status = new Status(StatusCode.Unavailable, "The server is shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Se produjo un error: {ex.Message}");
            }
        }
        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _subscriptor.DisposeAllServiceInstances();
        }
    }
}
