using AutoMapper;

using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Generated;

namespace VegaExpress.Agent.Services.Mapping
{
    public class BindServiceProfile : Profile
    {
        public BindServiceProfile()
        {
            CreateMap<Dictionary<string, string>, Google.Protobuf.Collections.MapField<string, string>>().ConvertUsing((src, dest) =>
            {
                var mapField = new Google.Protobuf.Collections.MapField<string, string>();
                foreach (var pair in src)
                {
                    mapField.Add(pair.Key, pair.Value);
                }
                return mapField;
            });

            CreateMap<Google.Protobuf.Collections.MapField<string, string>, Dictionary<string, string>>().ConvertUsing(src => src.ToDictionary(pair => pair.Key, pair => pair.Value));

            CreateMap<IEnumerable<NetworkAddressModel>,Google.Protobuf.Collections.RepeatedField<Generated.network_address>>().ConvertUsing((src, dest) =>
            {
                var mapField = new Google.Protobuf.Collections.RepeatedField<Generated.network_address>();
                foreach (var item in src)
                {
                    mapField.Add(new network_address 
                    { 
                        Name = item.Name,
                        MAC = item.MAC,
                        IPv4 = item.IPv4,
                        IPv6 = item.IPv6,
                        Speed = item.Speed
                    });
                }
                return mapField;
            });
            CreateMap<Google.Protobuf.Collections.RepeatedField<Generated.network_address>, IEnumerable<NetworkAddressModel>>().ConvertUsing(src => src.Select(item => new NetworkAddressModel
            {
                Name = item.Name,
                MAC = item.MAC,
                IPv4 = item.IPv4,
                IPv6 = item.IPv6,
                Speed = item.Speed
            }).ToList());
            
            CreateMap<Generated.service_state, ServiceState>().ConvertUsing(src => (ServiceState)(int)src);
            CreateMap<Generated.load_balancing_mode, LoadBalancingMode>().ConvertUsing(src => (LoadBalancingMode)(int)src);
            CreateMap<Generated.message_type, MessageType>().ConvertUsing(src => (MessageType)(int)src);
            CreateMap<Generated.instruction_type, InstructionType>().ConvertUsing(src => (InstructionType)(int)src);
            CreateMap<Generated.occurrence_type, OccurrenceType>().ConvertUsing(src => (OccurrenceType)(int)src);
            CreateMap<Generated.condition_type, ConditionType>().ConvertUsing(src => (ConditionType)(int)src);
            CreateMap<Generated.service_event_type, SessionEventType>().ConvertUsing(src => (SessionEventType)(int)src);
            CreateMap<Generated.load_balancer_event_type, LoadBalancerEventType>().ConvertUsing(src => (LoadBalancerEventType)(int)src);
            
            CreateMap<ServiceState, Generated.service_state>().ConvertUsing(src => (Generated.service_state)(int)src);
            CreateMap<LoadBalancingMode, Generated.load_balancing_mode>().ConvertUsing(src => (Generated.load_balancing_mode)(int)src);
            CreateMap<MessageType, Generated.message_type>().ConvertUsing(src => (Generated.message_type)(int)src);
            CreateMap<InstructionType, Generated.instruction_type>().ConvertUsing(src => (Generated.instruction_type)(int)src);
            CreateMap<OccurrenceType, Generated.occurrence_type>().ConvertUsing(src => (Generated.occurrence_type)(int)src);
            CreateMap<ConditionType, Generated.condition_type>().ConvertUsing(src => (Generated.condition_type)(int)src);
            CreateMap<SessionEventType, Generated.service_event_type>().ConvertUsing(src => (Generated.service_event_type)(int)src);
            CreateMap<LoadBalancerEventType, Generated.load_balancer_event_type>().ConvertUsing(src => (Generated.load_balancer_event_type)(int)src);

            CreateMap<Generated.metrics, MetricsModel>();
            CreateMap<Generated.instruction, InstructionModel>()
                .ForMember(dest => dest.InstructionType, opt => opt.MapFrom(src => (InstructionType)(int)src.Type))
                .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src.Parameters.ToDictionary(x => x.Key, x => x.Value)))
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => src.RequestDate.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate.ToDateTime().ToLocalTime()));

            CreateMap<Generated.occurrence, OccurrenceModel>()
                .ForMember(dest => dest.occurrenceType, opt => opt.MapFrom(src => (OccurrenceType)(int)src.Type))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details.ToDictionary(x => x.Key, x => x.Value)));

            CreateMap<Generated.gateway_routing_event_message, GatewayRoutingEventMessageModel>()
                .ForMember(dest => dest.RoutingEventType, opt => opt.MapFrom(src => (RoutingEventType)(int)src.EventType))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details.ToDictionary(pair => pair.Key, pair => pair.Value)));
            
            CreateMap<Generated.load_balancer_event_message, LoadBalancerEventMessageModel>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (LoadBalancerEventType)(int)src.EventType))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details.ToDictionary(pair => pair.Key, pair => pair.Value)));

            CreateMap<Generated.gateway_routing_service, GatewayRoutingServiceModel>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (ServiceState)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => src.LatestSession.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Server, opt => opt.MapFrom((src, dest) => new ServerModel
                {
                    HostName = src.Server.HostName,
                    UserName = src.Server.UserName,
                    OS = src.Server.OS,
                    OSArquitecture = src.Server.OSArquitecture,
                    ProcessArquitecture = src.Server.ProcessArquitecture,
                    RAM = src.Server.RAM,
                    NetworkAddresses = src.Server.NetworkAddresses?.Select(item => new NetworkAddressModel
                    {
                        Name = item.Name,
                        MAC = item.MAC,
                        IPv4 = item.IPv4,
                        IPv6 = item.IPv6,
                        Speed = item.Speed
                    }).ToArray(),
                    ServiceAgentUID = src.ServiceUID
                }));

            CreateMap<Generated.load_balancer_service, LoadBalancerServiceModel>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (ServiceState)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => src.LatestSession.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Server, opt => opt.MapFrom((src, dest) => new ServerModel
                {
                    HostName = src.Server.HostName,
                    UserName = src.Server.UserName,
                    OS = src.Server.OS,
                    OSArquitecture = src.Server.OSArquitecture,
                    ProcessArquitecture = src.Server.ProcessArquitecture,
                    RAM = src.Server.RAM,
                    NetworkAddresses = src.Server.NetworkAddresses?.Select(item => new NetworkAddressModel
                    {
                        Name = item.Name,
                        MAC = item.MAC,
                        IPv4 = item.IPv4,
                        IPv6 = item.IPv6,
                        Speed = item.Speed
                    }).ToArray(),
                    ServiceAgentUID = src.ServiceUID
                }));

            CreateMap<Generated.worker_service, WorkerServiceInstanceModel>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (ServiceState)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => src.LatestSession.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Server, opt => opt.MapFrom((src, dest) => new ServerModel
                {
                    HostName = src.Server.HostName,
                    UserName = src.Server.UserName,
                    OS = src.Server.OS,
                    OSArquitecture = src.Server.OSArquitecture,
                    ProcessArquitecture = src.Server.ProcessArquitecture,
                    RAM = src.Server.RAM,
                    NetworkAddresses = src.Server.NetworkAddresses.Select(item => new NetworkAddressModel
                    {
                        Name = item.Name,
                        MAC = item.MAC,
                        IPv4 = item.IPv4,
                        IPv6 = item.IPv6,
                        Speed = item.Speed
                    }).ToArray(),
                    ServiceAgentUID = src.ServiceUID
                }));

            CreateMap<Generated.client_service, ClientServiceModel>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (ServiceState)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => src.LatestSession.ToDateTime().ToLocalTime()))
                .ForMember(dest => dest.Server, opt => opt.MapFrom((src, dest) => new ServerModel
                {
                    HostName = src.Server.HostName,
                    UserName = src.Server.UserName,
                    OS = src.Server.OS,
                    OSArquitecture = src.Server.OSArquitecture,
                    ProcessArquitecture = src.Server.ProcessArquitecture,
                    RAM = src.Server.RAM,
                    NetworkAddresses = src.Server.NetworkAddresses.Select(item => new NetworkAddressModel
                    {
                        Name = item.Name,
                        MAC = item.MAC,
                        IPv4 = item.IPv4,
                        IPv6 = item.IPv6,
                        Speed = item.Speed
                    }).ToArray(),
                    ServiceAgentUID = src.ServiceUID
                }));

            CreateMap<MetricsModel, Generated.metrics>();
            CreateMap<InstructionModel, Generated.instruction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Generated.instruction_type)(int)src.InstructionType))
                .ForMember(dest => dest.Parameters, opt => opt.MapFrom((src, dest) =>
                {
                    var mapField = new Google.Protobuf.Collections.MapField<string, string>();
                    foreach (var pair in src.Parameters!)
                    {
                        mapField.Add(pair.Key, pair.Value);
                    }
                    return mapField;
                }))
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.RequestDate.ToUniversalTime())))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ExecutionDate.ToUniversalTime())));

            CreateMap<OccurrenceModel, Generated.occurrence>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Generated.occurrence_type)(int)src.occurrenceType))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ExecutionDate.ToUniversalTime())))
                .ForMember(dest => dest.Details, opt => opt.MapFrom((src, dest) =>
                {
                    var mapField = new Google.Protobuf.Collections.MapField<string, string>();
                    foreach (var pair in src.Details!)
                    {
                        mapField.Add(pair.Key, pair.Value);
                    }
                    return mapField;
                }));

            CreateMap<GatewayRoutingEventMessageModel, Generated.gateway_routing_event_message>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (Generated.routing_event_type)(int)src.RoutingEventType))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ExecutionDate.ToUniversalTime())))
                .ForMember(dest => dest.Details, opt => opt.MapFrom((src, dest) => 
                {
                    var mapField = new Google.Protobuf.Collections.MapField<string, string>();
                    foreach (var pair in src.Details!)
                    {
                        mapField.Add(pair.Key, pair.Value);
                    }
                    return mapField;
                }));
            CreateMap<LoadBalancerEventMessageModel, Generated.load_balancer_event_message>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (Generated.load_balancer_event_type)(int)src.EventType))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ExecutionDate.ToUniversalTime())))
                .ForMember(dest => dest.Details, opt => opt.MapFrom((src, dest) =>
                {
                    var mapField = new Google.Protobuf.Collections.MapField<string, string>();
                    foreach (var pair in src.Details!)
                    {
                        mapField.Add(pair.Key, pair.Value);
                    }
                    return mapField;
                }));            

            CreateMap<GatewayRoutingServiceModel, Generated.gateway_routing_service>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (Generated.service_state)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.LatestSession.ToUniversalTime())))
                .ForMember(dest => dest.Server, opt => opt.Ignore());

            CreateMap<LoadBalancerServiceModel, Generated.load_balancer_service>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (Generated.service_state)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.LatestSession.ToUniversalTime())))
                .ForMember(dest => dest.Server, opt => opt.Ignore());

            CreateMap<WorkerServiceInstanceModel, Generated.worker_service>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (Generated.service_state)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.LatestSession.ToUniversalTime())))
                .ForMember(dest => dest.Server, opt => opt.Ignore());

            CreateMap<ClientServiceModel, Generated.client_service>()
                .ForMember(dest => dest.ServiceState, opt => opt.MapFrom(src => (Generated.service_state)(int)src.ServiceState))
                .ForMember(dest => dest.LatestSession, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.LatestSession.ToUniversalTime())))
                .ForMember(dest => dest.Server, opt => opt.Ignore());
        }
    }
}