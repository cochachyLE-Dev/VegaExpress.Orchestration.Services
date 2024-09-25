using System.Collections.Concurrent;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Repository;
using VegaExpress.Agent.Generated;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Views;

namespace VegaExpress.Agent.Services
{
    public class LoadBalancer
    {
        //private ConcurrentDictionary<string, Service[]> _servicesInstances = new ConcurrentDictionary<string, Service[]>();        

        //private List<Service> _services = new List<Service>();
        //private int _next = 0;
        //private readonly MessageQueue<Service> _messageQueueService;
        //private readonly MessageQueue<ServiceManagement> _messageQueueServiceManagement;
        //private readonly IServiceRepository _serviceRepository;

        //public LoadBalancer(IServiceRepository serviceRepository, MessageQueue<ServiceManagement> messageQueueServiceManagement, MessageQueue<Service> messageQueueService)
        //{
        //    _serviceRepository = serviceRepository;
        //    _messageQueueServiceManagement = messageQueueServiceManagement;
        //    _messageQueueService = messageQueueService;

        //    _messageQueueService.Subscribe(nameof(LoadBalancer), (message) => 
        //    {
        //        int index = _services.FindIndex(x => x.ServiceAddress == message.ServiceAddress);

        //        if (index != -1)                
        //            _services[index] = message;                
        //        else                
        //            _services.Add(message);                
        //    });
        //}
        //public async Task<string> GetNextServiceAsync()
        //{
        //    int index = _services.FirstOrDefault();
        //    var serviceConfig = await _serviceRepository.GetByIdAsync(service.ServiceUID);

        //    if (service.ServiceTraffic < serviceConfig.ServiceTrafficLimit)
        //    {

        //    }
            
        //    if (!_services.Any(x => x.ServiceTraffic == 0) && _services.Count < serviceConfig.ServiceInstanceLimit)
        //    {
        //        _messageQueueServiceManagement.SendMessage(new ServiceManagement(service.ServiceUID, ServiceManagement.ActionType.StartService));                
        //    }

        //    _next = (_next + 1) % _services.Count;
        //    return service.ServiceAddress;
        //}
    }
}
