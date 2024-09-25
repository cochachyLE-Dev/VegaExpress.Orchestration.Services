using ReactiveUI;

using System.Reactive;
using System.Runtime.Serialization;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Services;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Utilities;

namespace VegaExpress.Agent.ViewModels
{
    public interface IMainViewModel { }
    [DataContract]
    public class MainViewModel: Router<ScenarioView>, IMainViewModel
    {
        public readonly IServiceProvider ServiceProvider;        
        public readonly Router<ScenarioFrame> FrameHost;
        public readonly LocalMessaging<ServiceManagement.ScheduledTask> MessageQueueSMScheduledTask;
        public readonly LocalMessaging<ProcessThreadModel> MessageQueueProcessThread;
        public readonly LocalMessaging<IServiceInstance> MessageQueueServiceInstance;
        public readonly LocalMessaging<WorkerServiceModel> MessageQueueService;
        public readonly LocalMessaging<GatewayRoutingServiceModel> MessageQueueGatewayService;
        public readonly LocalMessaging<LoadBalancerServiceModel> MessageQueueLoadBalancer;
        public readonly LocalMessaging<MessageQueueServiceModel> MessageQueueBusService;
        public readonly LocalMessaging<TaskSchedulerServiceModel> MessageQueueTaskSchedulerService;
        public readonly LocalMessaging<ServerModel> MessageQueueComputer;
        public readonly LocalMessaging<MessageLogModel> MessageQueueLogger;
        public MainViewModel(IServiceProvider serviceProvider, 
            LocalMessaging<ServiceManagement.ScheduledTask> messageQueueSMScheduledTask,
            LocalMessaging<ProcessThreadModel> messageQueueProcessThread,
            LocalMessaging<IServiceInstance> messageQueueServiceInstance,
            LocalMessaging<WorkerServiceModel> messageQueueService,
            LocalMessaging<GatewayRoutingServiceModel> messageQueueGatewayService,
            LocalMessaging<LoadBalancerServiceModel> messageQueueLoadBalancer,
            LocalMessaging<MessageQueueServiceModel> messageQueueBusService,
            LocalMessaging<TaskSchedulerServiceModel> messageQueueTaskSchedulerService,
            LocalMessaging<ServerModel> messageQueueComputer,
            LocalMessaging<MessageLogModel> messageQueueLogger,
            NavigateState navigateState) : base(navigateState.MainRouteState!)
        {
            ServiceProvider = serviceProvider;
            MessageQueueSMScheduledTask = messageQueueSMScheduledTask;

            MessageQueueProcessThread = messageQueueProcessThread;
            MessageQueueService = messageQueueService;
            MessageQueueServiceInstance = messageQueueServiceInstance;
            MessageQueueGatewayService = messageQueueGatewayService;
            MessageQueueLoadBalancer = messageQueueLoadBalancer;
            MessageQueueBusService = messageQueueBusService;
            MessageQueueTaskSchedulerService = messageQueueTaskSchedulerService;
            MessageQueueComputer = messageQueueComputer;
            MessageQueueLogger = messageQueueLogger;

            FrameHost = new Router<ScenarioFrame>(navigateState.BranchRouteState!);

            ShowMonitor = ReactiveCommand.Create(() =>
            {                
            });
        }
            
        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit> ShowMonitor { get; }
    }
}