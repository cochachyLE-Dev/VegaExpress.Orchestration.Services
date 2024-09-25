using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Generated;
using VegaExpress.Agent.Shared;
using Vaetech.Shell;
using Terminal.Gui;
using DynamicData;
using VegaExpress.Agent.Data.Enums;

namespace VegaExpress.Agent.Services
{
    public class ServiceManagement: BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IWorkerServiceInstanceRepository _workerServiceInstanceRepository;
        private readonly IGatewayRoutingServiceRepository _gatewayServiceInstanceRepository;
        private readonly IWorkerServiceRepository _serviceRepository;

        private readonly LocalMessaging<ServiceManagement.ScheduledTask> _messageQueueSMScheduledTask;        
        private readonly LocalMessaging<WorkerServiceModel> _messageQueueService;
        private readonly LocalMessaging<IServiceInstance> _messageQueueServiceInstance;
        private readonly LocalMessaging<MessageLogModel> _messageQueueLogger;

        private readonly ConcurrentDictionary<string, WorkerServiceModel> _services = new ConcurrentDictionary<string, WorkerServiceModel>();
        private readonly ConcurrentDictionary<string, Data.Interfaces.IServiceInstance[]> _servicesInstances = new ConcurrentDictionary<string, Data.Interfaces.IServiceInstance[]>();

        private readonly BindServiceSubscriptor _bindServiceSubscriptor;
        
        public ServiceManagement(IConfiguration configuration, BindServiceSubscriptor bindServiceSubscriptor, 
            IWorkerServiceRepository serviceRepository, 
            IWorkerServiceInstanceRepository serviceInstanceRepository, 
            IGatewayRoutingServiceRepository gatewayServiceInstanceRepository,
            LocalMessaging<ServiceManagement.ScheduledTask> messageQueueSMScheduledTask,            
            LocalMessaging<IServiceInstance> messageQueueServiceInstance,
            LocalMessaging<WorkerServiceModel> messageQueueService,
            LocalMessaging<MessageLogModel> messageQueueLogger
            )
        {
            _configuration = configuration;
            _bindServiceSubscriptor = bindServiceSubscriptor;
            _serviceRepository = serviceRepository;
            _workerServiceInstanceRepository = serviceInstanceRepository;
            _gatewayServiceInstanceRepository = gatewayServiceInstanceRepository;

            _messageQueueSMScheduledTask = messageQueueSMScheduledTask;            
            _messageQueueService = messageQueueService;
            _messageQueueServiceInstance = messageQueueServiceInstance;
            _messageQueueLogger = messageQueueLogger;            

            _messageQueueSMScheduledTask.Subscribe(nameof(ServiceManagement), message =>
            {
                switch (message.ActionType)
                {
                    case ActionType.LoadAllService:
                        _ = LoadAllWorkerServices();
                        break;
                    case ActionType.AddOrUpdateService:
                        AddOrUpdateService(message.Service!);
                        break;
                    case ActionType.AddOrUpdateGatewayService:
                        AddOrUpdateService((GatewayRoutingServiceModel)message.ServiceInstance!);
                        break;
                    case ActionType.RemoveService:
                        RemoveService(message.Service!.ServiceUID!);
                        break;
                    case ActionType.AddOrUpdateServiceInstance:
                        AddOrUpdateServiceInstance(message.ServiceInstance!);
                        break;
                    case ActionType.StartNewServiceInstance:
                        StartNewServiceInstance(message.Service!.ServiceUID!);
                        break;
                    case ActionType.StartServiceInstance:
                        StartServiceInstance(message.ServiceInstance!.ServiceAddress!);
                        break;
                    case ActionType.StopServiceInstance:
                        StopServiceInstance(message.ServiceInstance!.ServiceAddress!);
                        break;
                    case ActionType.RestartServiceInstance:
                        RestartServiceInstance(message.ServiceInstance!.ServiceAddress!);
                        break;

                    case ActionType.StartAllServiceInstances:
                        StartAllWorkerServiceInstances(message.Service!.ServiceUID);
                        break;
                    case ActionType.StopAllServiceInstances:
                        StopAllWorkerServiceInstances(message.Service!.ServiceUID).ToArray();
                        break;
                    case ActionType.RestartAllServiceInstances:
                        RestartAllWorkerServiceInstances(message.Service!.ServiceUID);
                        break;

                    case ActionType.StartServiceMonitor:
                        StartServiceInstanceMonitor(message.ServiceInstance!.ServiceAddress!);
                        break;
                    case ActionType.StopServiceMonitor:
                        StopServiceInstanceMonitor(message.ServiceInstance!.ServiceAddress!);
                        break;
                }
            });

            _ = LoadAllWorkerServices();
        }
        
        internal async Task LoadAllWorkerServices() 
        {   
            var list = await _serviceRepository!.GetAllAsync();
            var gatewayRoutings = await _gatewayServiceInstanceRepository!.GetAllAsync();

            foreach (var service in list)
            {
                _services.AddOrUpdate(service.ServiceUID!, service, (key, oldValue) => service);
                _messageQueueService.SendMessage(service);

                var addresses = GetAvailableHttpAddresses(service!);
                List<IServiceInstance> serviceInstances = new List<IServiceInstance>();
                
                foreach (var address in addresses)
                {
                    var serviceInstance = new WorkerServiceInstanceModel
                    {
                        ServiceUID = Guid.NewGuid().ToString(),
                        ServiceName = service.ServiceName,
                        ServiceLocation = service.ServiceLocation,
                        ServiceVersion = service.ServiceVersion,
                        ServiceAddress = address,
                        ServiceAgentUID = service.ServiceUID,
                        ServiceState = ServiceState.Stopped,
                        Color = service.Color,
                    };
                    serviceInstances.Add(serviceInstance);
                    _messageQueueServiceInstance.SendMessage(serviceInstance);                    
                }

                var gatewayRoutingServices = gatewayRoutings.Where(x => x.ServiceAgentUID == service.ServiceUID);
                foreach (var gatewayRoute in gatewayRoutingServices)
                {
                    var serviceInstance = new GatewayRoutingServiceModel
                    {
                        ServiceUID = gatewayRoute.ServiceUID,
                        ServiceName = gatewayRoute.ServiceName,
                        ServiceLocation = gatewayRoute.ServiceLocation,
                        ServiceVersion = gatewayRoute.ServiceVersion,
                        ServiceAddress = gatewayRoute.ServiceAddress,
                        ServiceAgentUID = gatewayRoute.ServiceAgentUID,
                        ServiceState = ServiceState.Stopped,
                        Color = service.Color,
                    };
                    serviceInstances.Add(serviceInstance);
                    _messageQueueServiceInstance.SendMessage(serviceInstance);
                }

                _servicesInstances.AddOrUpdate(service.ServiceUID!, serviceInstances.ToArray(), (key, oldValue) => serviceInstances.ToArray());
            }
        }

        internal void AddOrUpdateService(GatewayRoutingServiceModel service)
        {
            if (_servicesInstances.TryGetValue(service.ServiceAgentUID!, out IServiceInstance[]? serviceInstances))
            {
                if (serviceInstances == null || !serviceInstances.Any(x => x.ServiceAddress == service.ServiceAddress))
                {
                    List<IServiceInstance> instances = new List<IServiceInstance>(serviceInstances!);
                    instances.Add(service);

                    _servicesInstances.AddOrUpdate(service.ServiceAgentUID!, instances.ToArray(), (k, v) => instances.ToArray());
                    _messageQueueServiceInstance.SendMessage(service);
                }
            }
        }
        internal void AddOrUpdateService(WorkerServiceModel service)
        {
            if(_services.TryGetValue(service.ServiceUID!, out _))            
            {
                int count = 0;
                foreach (var _ in StopAllWorkerServiceInstances(service.ServiceUID, removeItems: true))
                { 
                    Thread.Sleep(200 * count++);
                }
            }

            _services.AddOrUpdate(service.ServiceUID!, service, (key, oldValue) => service);
            _messageQueueService.SendMessage(service);

            var addresses = GetAvailableHttpAddresses(service!, allPorts: true);
            var serviceInstances = new WorkerServiceInstanceModel[addresses.Length];

            int i = 0;
            foreach (var address in addresses)
            {
                serviceInstances[i] = new WorkerServiceInstanceModel();
                serviceInstances[i].ServiceUID = Guid.NewGuid().ToString();
                serviceInstances[i].ServiceName = service.ServiceName;
                serviceInstances[i].ServiceLocation = service.ServiceLocation;
                serviceInstances[i].ServiceVersion = service.ServiceVersion;
                serviceInstances[i].ServiceAddress = address;
                serviceInstances[i].ServiceAgentUID = service.ServiceUID;
                serviceInstances[i].ServiceState = ServiceState.Stopped;
                serviceInstances[i].ServiceLocation = service.ServiceLocation;
                serviceInstances[i].Color = service.Color;

                _messageQueueServiceInstance.SendMessage((IServiceInstance)serviceInstances[i]);

                i++;
            }

            _servicesInstances.AddOrUpdate(service.ServiceUID!, serviceInstances, (key, oldValue) => serviceInstances);

            if (service.ServiceStartType == nameof(ServiceStartType.Automatic))
            {
                StartAllWorkerServiceInstances(service.ServiceUID);
            }

        }
        internal async void RemoveService(string serviceUID)
        {
            if (_services.TryGetValue(serviceUID!, out WorkerServiceModel? service))
            {
                await _serviceRepository.DeleteAsync(service.ServiceUID!);
            }

            _services.Remove(service!.ServiceUID!, out _);
            _messageQueueService.SendMessage(service, MessageAction.Remove);
        }

        internal async void AddOrUpdateServiceInstance(IServiceInstance service)
        {
            var services = _servicesInstances.SelectMany(si => si.Value).ToList();
            int index = services.FindIndex(x => x.ServiceAddress == service.ServiceAddress);

            if (index != -1)
            {
                services[index] = service;
                switch (service)
                {
                    case WorkerServiceInstanceModel workerServiceInstanceModel:
                        await _workerServiceInstanceRepository.UpdateAsync(workerServiceInstanceModel);
                        break;
                    case GatewayRoutingServiceModel gatewayRoutingServiceModel:
                        await _gatewayServiceInstanceRepository.UpdateAsync(gatewayRoutingServiceModel);
                        break;
                }
            }
            else
            {
                services.Add(service);
                switch (service)
                {
                    case WorkerServiceInstanceModel workerServiceInstanceModel:
                        await _workerServiceInstanceRepository.CreateAsync(workerServiceInstanceModel);
                        break;
                    case GatewayRoutingServiceModel gatewayRoutingServiceModel:
                        await _gatewayServiceInstanceRepository.CreateAsync(gatewayRoutingServiceModel);
                        break;
                }
            }

            lock (this)
            {
                var updateServices = services.Where(x => x.ServiceAgentUID == service.ServiceAgentUID).ToArray();
                _servicesInstances.AddOrUpdate(service.ServiceAgentUID!, updateServices, (key, oldValue) => updateServices);
                _messageQueueServiceInstance.SendMessage(service);
            }
        }

        internal void StartNewServiceInstance(string serviceAgentUid)
        {            
            if (_services.TryGetValue(serviceAgentUid, out WorkerServiceModel? service))
            {
                string directory = Path.GetDirectoryName(service.ServiceLocation)!;
                string fileName = Path.GetFileName(service.ServiceLocation)!;

                if (!Path.HasExtension(".exe"))
                {
                    throw new ArgumentException(fileName);
                }

                var addresses = GetAvailableHttpAddresses(service);
                string address = null!;

                for (;;)
                {
                    address = addresses.FirstOrDefault()!;
                    if (address == null)
                        break;
                    int port = Convert.ToInt32(address!.Split(":")[2]);

                    if (!IsPortInUse(port))
                        break;
                }

                if (address == null)
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        MessageBox.Query("Message", "Los puertos del servicio están en uso", "_Ok");
                    });
                }

                WorkerServiceInstanceModel serviceInstance = new WorkerServiceInstanceModel();
                serviceInstance.ServiceName = service.ServiceName;
                serviceInstance.ServiceLocation = service.ServiceLocation;
                serviceInstance.ServiceVersion = service.ServiceVersion;
                serviceInstance.ServiceAddress = address;
                serviceInstance.ServiceAgentUID = service.ServiceUID;
                serviceInstance.ServiceState = ServiceState.Started;

                AddOrUpdateServiceInstance(serviceInstance);

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    string serviceAgentAddress = _configuration["service-address"]!;
                    PShell.Execute(service.ServiceLocation!, $" --service-address {address} --service-agent-address {serviceAgentAddress} --service-agent-uid {serviceInstance.ServiceAgentUID}", 
                        (log) => _messageQueueLogger.SendMessage(/*address!,*/ new MessageLogModel(log.Line, log.Output)));
                }));
                thread.Start();
            }
        }
        internal void StartServiceInstance(string serviceInstanceAddress)
        {
            var serviceInstance = GetServiceInstance(serviceInstanceAddress);
            if (serviceInstance != null && _servicesInstances.TryGetValue(serviceInstance.ServiceAgentUID!, out IServiceInstance[]? serviceInstances))
            {
                serviceInstance!.ServiceState = ServiceState.Running;

                int indexOf = serviceInstances.IndexOf(serviceInstance);
                serviceInstances[indexOf] = serviceInstance;

                _servicesInstances.AddOrUpdate(serviceInstance.ServiceAgentUID!, serviceInstances, (k, v) => serviceInstances);
                _messageQueueServiceInstance.SendMessage(serviceInstance);

                serviceInstance.ServiceLocation = _services[serviceInstance.ServiceAgentUID!].ServiceLocation;

                switch (serviceInstance)
                {
                    case WorkerServiceInstanceModel:
                        {
                            Thread thread = new Thread(new ThreadStart(() =>
                            {
                                string serviceAgentAddress = _configuration["service-address"]!;
                                PShell.Execute(serviceInstance.ServiceLocation!, $" --service-address {serviceInstance.ServiceAddress} --service-agent-address {serviceAgentAddress} --service-agent-uid {serviceInstance.ServiceAgentUID}",
                                    (log) => _messageQueueLogger.SendMessage(/*address!,*/ new MessageLogModel(log.Line, log.Output)));
                            }));
                            thread.Start();
                        }
                        break;
                    case GatewayRoutingServiceModel:
                        {
                            Thread thread = new Thread(new ThreadStart(() =>
                            {
                                string serviceAgentAddress = _configuration["service-address"]!;
                                PShell.Execute(serviceInstance.ServiceLocation!, $" --service-address {serviceInstance.ServiceAddress} --service-agent-address {serviceAgentAddress} --service-agent-uid {serviceInstance.ServiceAgentUID}",
                                    (log) => _messageQueueLogger.SendMessage(/*address!,*/ new MessageLogModel(log.Line, log.Output)));
                            }));
                            thread.Start();
                        }
                        break;
                }
            }
        }

        internal void StopServiceInstance(string serviceInstanceAddress, bool removeItem = false)
        {
            var serviceInstance = GetServiceInstance(serviceInstanceAddress);
            if(serviceInstance != null && _servicesInstances.TryGetValue(serviceInstance.ServiceAgentUID!, out IServiceInstance[]? serviceInstances))
            {
                serviceInstance!.ServiceState = ServiceState.Running;

                int indexOf = serviceInstances.IndexOf(serviceInstance);
                serviceInstances[indexOf] = serviceInstance;

                _servicesInstances.AddOrUpdate(serviceInstance.ServiceAgentUID!, serviceInstances, (k, v) => serviceInstances);

                _messageQueueServiceInstance.SendMessage(serviceInstance);

                switch (serviceInstance)
                {
                    case WorkerServiceInstanceModel workerServiceInstanceModel:
                        {
                            var session = new session
                            {
                                Service = new service
                                {
                                    EventType = removeItem ? service_event_type.SetFinishingStream : service_event_type.SetEndingStream,
                                    WorkerService = new worker_service { ServiceAddress = serviceInstance.ServiceAddress }
                                }
                            };                            
                            _bindServiceSubscriptor.SessionServerStreamWriter(serviceInstance.ServiceAddress!, session);
                        }
                        break;
                    case GatewayRoutingServiceModel gatewayRoutingServiceModel:
                        {
                            var session = new session
                            {
                                Service = new service
                                {
                                    EventType = removeItem ? service_event_type.SetFinishingStream : service_event_type.SetEndingStream,
                                    GatewayRoutingService = new gateway_routing_service { ServiceAddress = serviceInstance.ServiceAddress }
                                }
                            };                            
                            _bindServiceSubscriptor.SessionServerStreamWriter(serviceInstance.ServiceAddress!, session);
                        }
                        break;
                }
            }
        }

        internal void RestartServiceInstance(string serviceUid)
        {
            StopServiceInstance(serviceUid);
            StartServiceInstance(serviceUid);
        }

        private void RestartAllWorkerServiceInstances(string? serviceAgentUID)
        {
            StopAllWorkerServiceInstances(serviceAgentUID).ToArray();
            StartAllWorkerServiceInstances(serviceAgentUID);
        }

        private IEnumerable<IServiceInstance> StopAllWorkerServiceInstances(string? serviceAgentUID, bool removeItems = false)
        {
            if (_servicesInstances.TryGetValue(serviceAgentUID!, out IServiceInstance[]? serviceInstances))
            {
                var workerServiceInstances = serviceInstances.OfType<WorkerServiceInstanceModel>();

                foreach (var workerServiceInstance in workerServiceInstances)
                {
                    if (workerServiceInstance.ServiceState == ServiceState.Started)
                    {
                        StopServiceInstance(workerServiceInstance.ServiceAddress!, removeItems);
                        yield return workerServiceInstance;
                    }
                    else if(removeItems)
                    {
                        _messageQueueServiceInstance.SendMessage(workerServiceInstance, MessageAction.Remove);
                    }                    
                }
            }
        }

        private void StartAllWorkerServiceInstances(string? serviceAgentUID)
        {
            if (_services.TryGetValue(serviceAgentUID!, out WorkerServiceModel? service))
            {                 
                if (_servicesInstances.TryGetValue(serviceAgentUID!, out IServiceInstance[]? serviceInstances))
                {
                    var workerServiceInstances = serviceInstances.OfType<WorkerServiceInstanceModel>();
                    foreach (var workerServiceInstance in workerServiceInstances)
                    {
                        if (workerServiceInstance.ServiceState != ServiceState.Started)
                        {
                            StartServiceInstance(workerServiceInstance.ServiceAddress!);
                            Thread.Sleep(100);
                        }
                    }
                }
            }            
        }

        internal void StartServiceInstanceMonitor(string serviceInstanceAddress)
        {                
            _bindServiceSubscriptor.StartServiceInstanceMonitor(serviceInstanceAddress);
        }
        internal void StopServiceInstanceMonitor(string serviceInstanceAddress)
        {
            _bindServiceSubscriptor.StopServiceInstanceMonitor(serviceInstanceAddress);
        }

        private string[] GetAvailableHttpAddresses(WorkerServiceModel service, bool allPorts = true)
        {
            List<int> ports = GetAllPorts(service.ServicePortRange!);
            string serviceAddressWithColon = string.Concat(service.ServiceAddress, ":");
            HashSet<string> portSet = new HashSet<string>(ports.Select(p => serviceAddressWithColon + p));

            if (allPorts) return portSet.ToArray();

            var addressListener = _servicesInstances.SelectMany(x => x.Value).Where(x => x.ServiceState == ServiceState.Stopped).Select(x => x.ServiceAddress);            
            var addresses = portSet
                .Where(pl => !addressListener.Contains(pl))
                .ToArray();

            return addresses;
        }
        private bool IsPortInUse(int port)
        {
            bool inUse = false;

            TcpListener tcpListener = null!;
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
            }
            catch (SocketException)
            {
                inUse = true;
            }
            finally
            {
                tcpListener?.Stop();
            }

            return inUse;
        }
        private List<int> GetAllPorts(params string[] ports)
        {
            List<int> numbers = new List<int>();

            foreach (var input in ports)
            {
                foreach (var part in input.Split(','))
                {
                    if (part.Contains('-'))
                    {
                        var rangeParts = part.Split('-').Select(int.Parse).ToArray();
                        numbers.AddRange(Enumerable.Range(rangeParts[0], rangeParts[1] - rangeParts[0] + 1));
                    }
                    else
                    {
                        numbers.Add(int.Parse(part));
                    }
                }
            }
            return numbers;
        }

        public Data.Interfaces.IServiceInstance GetServiceInstance(string serviceInstanceAddress)
        {
            var service = _servicesInstances.SelectMany(si => si.Value).FirstOrDefault(i => i.ServiceAddress == serviceInstanceAddress);
            return service!;
        }
        public IEnumerable<Data.Interfaces.IServiceInstance> GetAllServices()
        {
            var services = _servicesInstances.SelectMany(si => si.Value);
            return services!;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public class ScheduledTask
        {
            public ScheduledTask(WorkerServiceModel service, ActionType actionType)
            {
                Service = service;
                ActionType = actionType;
            }            
            public ScheduledTask(IServiceInstance service, ActionType actionType)
            {
                ServiceInstance = service;
                ActionType = actionType;
            }
            public IServiceInstance? ServiceInstance { get; protected set; }
            public WorkerServiceModel? Service { get; protected set; }            
            public ActionType ActionType { get; protected set; }
        }
        public enum ActionType
        {
            None = 0,
            LoadAllService = 1,

            AddOrUpdateService = 10,
            AddOrUpdateGatewayService = 11,
            AddOrUpdateServiceInstance = 12,
            RemoveService = 13,
            RemoveGatewayService = 14,

            StartServiceInstance = 20,
            StopServiceInstance = 21,
            RestartServiceInstance = 22,

            StartNewServiceInstance = 23,
            StartAllServiceInstances = 24,
            StopAllServiceInstances = 25,
            RestartAllServiceInstances = 26,

            StartServiceMonitor = 30,
            StopServiceMonitor = 31
        }
    }
}