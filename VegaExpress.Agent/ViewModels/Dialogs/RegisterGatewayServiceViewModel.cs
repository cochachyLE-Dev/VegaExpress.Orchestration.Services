using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Reactive;

using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Interfaces;

using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Data.Repository;
using VegaExpress.Agent.Services;
using VegaExpress.Agent.Shared;

namespace VegaExpress.Agent.ViewModels.Dialog
{
    public interface IRegisterGatewayServiceViewModel
    {        
        public string? ServiceUID { get; set; }     
        public string? ServiceName { get; set; }        
        public string? ServiceLocation { get; set; }        
        public string? ServiceVersion { get; set; }        
        public string? ServiceAddress { get; set; }        
        public string? AgentServiceUID { get; set; }        
        public string? ProcessName { get; set; }        
        public int PID { get; set; }        
        public DateTime LastSession { get; set; }        
        public ServiceState ServiceState { get; set; }
        public string? ServiceStartType { get; set; }
        public string? Color { get; set; }
        public IObservable<bool>? CanExecute { get; }
        public void LoadService(string serviceUID);
        ReactiveCommand<Unit, Unit>? Save { get; }
        ReactiveCommand<Unit, Unit>? Cancel { get; }
    }
    internal class RegisterGatewayServiceViewModel : ReactiveObject, IRegisterGatewayServiceViewModel
    {
        const string ServiceVersionDefault = "1.0.0.0";
        const string ServiceAddressDefault = "https://127.0.0.1:8080";        
        const string? ServiceStateDefault = "Automatic";
        const string? ColorDefault = "Orange";

        [Reactive]
        public string? ServiceUID { get; set; }
        [Reactive]
        public string? ServiceName { get; set; }
        [Reactive]
        public string? ServiceLocation { get; set; }
        [Reactive]
        public string? ServiceVersion { get; set; }
        [Reactive]
        public string? ServiceAddress { get; set; }
        [Reactive]
        public string? AgentServiceUID { get; set; }
        [Reactive]
        public string? ProcessName { get; set; }
        [Reactive]
        public int PID { get; set; }
        [Reactive]
        public DateTime LastSession { get; set; }
        [Reactive]
        public ServiceState ServiceState { get; set; }
        [Reactive]
        public string? ServiceStartType { get; set; }
        [Reactive]
        public string? Color { get; set; }

        public ReactiveCommand<Unit, Unit>? Save { get; }

        public ReactiveCommand<Unit, Unit>? Cancel { get; }

        public IObservable<bool>? CanExecute { get; }

        private readonly IGatewayRoutingServiceRepository gatewayServiceRepository;        

        public readonly LocalMessaging<ServiceManagement.ScheduledTask> MessageQueueSMScheduledTask;
        
        public RegisterGatewayServiceViewModel(IGatewayRoutingServiceRepository gatewayServiceRepository, LocalMessaging<ServiceManagement.ScheduledTask> messageQueueSMScheduledTask)
        {
            this.gatewayServiceRepository = gatewayServiceRepository;  
            this.MessageQueueSMScheduledTask = messageQueueSMScheduledTask;
            
            ServiceUID = Guid.NewGuid().ToString();
            ServiceVersion = ServiceVersionDefault;
            ServiceAddress = ServiceAddressDefault;
            ServiceStartType = ServiceStateDefault;
            Color = ColorDefault;

            var canExecute = this.WhenAnyValue(
                x => x.ServiceUID,
                x => x.ServiceName,
                x => x.ServiceVersion,
                x => x.ServiceLocation,
                x => x.AgentServiceUID,
                (serviceUID, serviceName, serviceVersion, serviceLocation, agentServiceUID) =>
                    !string.IsNullOrWhiteSpace(serviceUID) &&
                    !string.IsNullOrWhiteSpace(serviceName) &&
                    !string.IsNullOrWhiteSpace(serviceVersion) &&
                    !string.IsNullOrWhiteSpace(serviceLocation) &&
                    !string.IsNullOrWhiteSpace(agentServiceUID));

            Save = ReactiveCommand.CreateFromTask(async () =>
            {
                GatewayRoutingServiceModel gatewayServiceModel = new GatewayRoutingServiceModel();
                gatewayServiceModel.ServiceUID = ServiceUID;
                gatewayServiceModel.ServiceName = ServiceName;
                gatewayServiceModel.ServiceLocation = ServiceLocation;
                gatewayServiceModel.ServiceVersion = ServiceVersion;
                gatewayServiceModel.ServiceAddress = ServiceAddress;
                gatewayServiceModel.ServiceAgentUID = AgentServiceUID;
                gatewayServiceModel.ServiceState = ServiceState;
                gatewayServiceModel.ServiceStartType = ServiceStartType;
                gatewayServiceModel.Color = Color;

                await this.gatewayServiceRepository.CreateAsync(gatewayServiceModel);
            }, canExecute);

            Save.Subscribe(_ =>
            {
                GatewayRoutingServiceModel gatewayServiceModel = new GatewayRoutingServiceModel();
                gatewayServiceModel.ServiceUID = ServiceUID;
                gatewayServiceModel.ServiceName = ServiceName;
                gatewayServiceModel.ServiceLocation = ServiceLocation;
                gatewayServiceModel.ServiceVersion = ServiceVersion;
                gatewayServiceModel.ServiceAddress = ServiceAddress;
                gatewayServiceModel.ServiceAgentUID = AgentServiceUID;
                gatewayServiceModel.ServiceState = ServiceState;
                gatewayServiceModel.ServiceStartType = ServiceStartType;
                gatewayServiceModel.Color = Color;

                MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(gatewayServiceModel, ServiceManagement.ActionType.AddOrUpdateGatewayService));
            });

            Cancel = ReactiveCommand.Create(() => { });
        }

        public async void LoadService(string serviceUID)
        {
            GatewayRoutingServiceModel service = await gatewayServiceRepository.GetByIdAsync(serviceUID);            
            ServiceUID = service.ServiceUID;
            ServiceName = service.ServiceName;
            ServiceLocation = service.ServiceLocation;
            ServiceVersion = service.ServiceVersion;
            ServiceAddress = service.ServiceAddress;
            AgentServiceUID = service.ServiceAgentUID;
            ProcessName = service.ProcessName;
            PID = service.ProcessID;
            LastSession = service.LatestSession;
            ServiceState = service.ServiceState;
            ServiceStartType = service.ServiceStartType;
            Color = service.Color;
        }
    }
}
