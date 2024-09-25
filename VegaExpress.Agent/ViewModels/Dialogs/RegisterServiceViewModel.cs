using Google.Api;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Reactive;
using System.Reactive.Disposables;

using VegaExpress.Agent.Data.Interfaces;

using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Data.Repository;
using VegaExpress.Agent.Services;
using VegaExpress.Agent.Shared;

namespace VegaExpress.Agent.ViewModels.Dialog
{
    public interface IRegisterServiceViewModel
    {  
        public string? ServiceUID { get; set; }        
        public string? ServiceName { get; set; }        
        public string? ServiceVersion { get; set; }       
        public string? ServiceAddress { get; set; }        
        public string? GatewayServiceAddress { get; set; }        
        public string? ServicePortRange { get; set; }        
        public int ServiceTrafficLimit { get; set; }        
        public int ServiceErrorRateLimit { get; set; }        
        public int ServiceInstanceLimit { get; set; }        
        public string? ServiceLocation { get; set; }        
        public string? ServiceStartType { get; set; }        
        public string? Color { get; set; }

        public void LoadService(string serviceUID);

        public IObservable<bool> CanExecute { get; }
        ReactiveCommand<Unit, Unit>? Save { get; }
        ReactiveCommand<Unit, Unit>? Cancel { get; }        
    }
    internal class RegisterServiceViewModel : ReactiveObject, IRegisterServiceViewModel
    {
        const string ServiceVersionDefault = "1.0.0.0";
        const string ServiceAddressDefault = "https://127.0.0.1";
        const string ServicePortRangeDefault = "8080-8090";
        const string GatewayServiceAddressDefault = "https://127.0.0.1:8080";
        const int ServiceTrafficLimitDefault = 1;
        const int ServiceErrorRateLimitDefault = 3;
        const int ServiceInstanceLimitDefault = 5;
        const string? ServiceStateDefault = "Automatic";
        const string? ColorDefault = "Orange";

        [Reactive]
        public string? ServiceUID { get; set; }
        [Reactive]
        public string? ServiceName { get; set; }
        [Reactive]
        public string? ServiceVersion { get; set; }
        [Reactive]
        public string? ServiceAddress { get; set; }
        [Reactive]
        public string? GatewayServiceAddress { get; set; }
        [Reactive]
        public string? ServicePortRange { get; set; }
        [Reactive]
        public int ServiceTrafficLimit { get; set; }
        [Reactive]
        public int ServiceErrorRateLimit { get; set; }
        [Reactive]
        public int ServiceInstanceLimit { get; set; }
        [Reactive]
        public string? ServiceLocation { get; set; }
        [Reactive]
        public string? ServiceStartType { get; set; }
        [Reactive]
        public string? Color { get; set; }

        public ReactiveCommand<Unit, Unit>? Save { get; }

        public ReactiveCommand<Unit, Unit>? Cancel { get; }

        public IObservable<bool> CanExecute { get; }

        private readonly IWorkerServiceRepository serviceRepository;

        public readonly LocalMessaging<ServiceManagement.ScheduledTask> MessageQueueSMScheduledTask;

        public RegisterServiceViewModel(IWorkerServiceRepository serviceRepository, LocalMessaging<ServiceManagement.ScheduledTask> messageQueueSMScheduledTask)
        {
            this.serviceRepository = serviceRepository;
            this.MessageQueueSMScheduledTask = messageQueueSMScheduledTask;

            ServiceUID = Guid.NewGuid().ToString();
            GatewayServiceAddress = GatewayServiceAddressDefault;
            ServiceVersion = ServiceVersionDefault;
            ServiceAddress = ServiceAddressDefault;
            ServicePortRange = ServicePortRangeDefault;
            ServiceTrafficLimit = ServiceTrafficLimitDefault;
            ServiceErrorRateLimit = ServiceErrorRateLimitDefault;
            ServiceInstanceLimit = ServiceInstanceLimitDefault;
            ServiceStartType = ServiceStateDefault;
            Color = ColorDefault;

            CanExecute = this.WhenAnyValue(
                x => x.ServiceUID,
                x => x.ServiceName,
                x => x.ServiceVersion,
                x => x.ServiceAddress,
                x => x.ServicePortRange,
                x => x.GatewayServiceAddress,                
                x => x.ServiceLocation,                
                (serviceUID, serviceName, serviceVersion, serviceAddress, servicePortRange, gateway, serviceLocation) =>
                    !string.IsNullOrWhiteSpace(serviceUID) &&
                    !string.IsNullOrWhiteSpace(serviceName) &&
                    !string.IsNullOrWhiteSpace(serviceVersion) &&
                    !string.IsNullOrWhiteSpace(serviceAddress) &&
                    !string.IsNullOrWhiteSpace(servicePortRange) &&
                    !string.IsNullOrWhiteSpace(gateway) &&                    
                    !string.IsNullOrWhiteSpace(serviceLocation));
            
            Save = ReactiveCommand.CreateFromTask(async () =>
            {
                WorkerServiceModel serviceModel = new WorkerServiceModel();
                serviceModel.ServiceUID = ServiceUID;
                serviceModel.ServiceName = ServiceName;
                serviceModel.ServiceVersion = ServiceVersion;
                serviceModel.ServiceAddress = ServiceAddress;
                serviceModel.ServicePortRange = ServicePortRange;
                serviceModel.GatewayServiceAddress = GatewayServiceAddress;
                serviceModel.ServiceTrafficLimit = ServiceTrafficLimit;
                serviceModel.ServiceErrorRateLimit = ServiceErrorRateLimit;
                serviceModel.ServiceInstanceLimit = ServiceInstanceLimit;
                serviceModel.ServiceLocation = ServiceLocation;
                serviceModel.ServiceStartType = ServiceStartType;
                serviceModel.Color = Color;

                var service = await serviceRepository.GetByIdAsync(ServiceUID!);
                
                Task<bool> task;                
                if (service != null)                
                    task = serviceRepository.UpdateAsync(serviceModel);                
                else                
                    task = serviceRepository.CreateAsync(serviceModel!);
                
               await task;
            },CanExecute);

            Save.Subscribe(_ => 
            {
                WorkerServiceModel serviceModel = new WorkerServiceModel();
                serviceModel.ServiceUID = ServiceUID;
                serviceModel.ServiceName = ServiceName;
                serviceModel.ServiceVersion = ServiceVersion;
                serviceModel.ServiceAddress = ServiceAddress;
                serviceModel.ServicePortRange = ServicePortRange;
                serviceModel.GatewayServiceAddress = GatewayServiceAddress;
                serviceModel.ServiceTrafficLimit = ServiceTrafficLimit;
                serviceModel.ServiceErrorRateLimit = ServiceErrorRateLimit;
                serviceModel.ServiceInstanceLimit = ServiceInstanceLimit;
                serviceModel.ServiceLocation = ServiceLocation;
                serviceModel.ServiceStartType = ServiceStartType;
                serviceModel.Color = Color;

                MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceModel, ServiceManagement.ActionType.AddOrUpdateService));
            });

            Cancel = ReactiveCommand.Create(() => { });
        }
        public async void LoadService(string serviceUID)
        {
            WorkerServiceModel service = await serviceRepository.GetByIdAsync(serviceUID);
            ServiceUID = service.ServiceUID;
            ServiceName = service.ServiceName;
            ServiceVersion = service.ServiceVersion;
            ServiceAddress = service.ServiceAddress;
            ServicePortRange = service.ServicePortRange;
            GatewayServiceAddress = service.GatewayServiceAddress;
            ServiceTrafficLimit = service.ServiceTrafficLimit;
            ServiceErrorRateLimit = service.ServiceErrorRateLimit;
            ServiceInstanceLimit = service.ServiceInstanceLimit;
            ServiceLocation = service.ServiceLocation;
            ServiceStartType = service.ServiceStartType;
            Color = service.Color;
        }
    }
}
