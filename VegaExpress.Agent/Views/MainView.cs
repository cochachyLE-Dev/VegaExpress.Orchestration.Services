using ReactiveUI;

using Splat;

using System.Reactive.Disposables;
using System.Runtime.InteropServices;

using Terminal.Gui;

using Vaetech.Shell;

using VegaExpress.Agent.Constants;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Extentions;
using VegaExpress.Agent.Generated;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.ViewModels;
using VegaExpress.Agent.ViewModels.Dialog;
using VegaExpress.Agent.Services;
using DynamicData;
using Google.Api;
using VegaExpress.Agent.Views.Dialogs;
using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Controls;
using System.Linq.Expressions;

namespace VegaExpress.Agent.Views
{
    public class MainView : ScenarioView, IViewFor<MainViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        readonly CancellationToken _cancellationToken = new CancellationToken();        

        const bool tableStyleAlwaysShowHeaders = true;
        const bool tableStyleShowHorizontalHeaderOverline = false;
        const bool tableStyleShowVerticalHeaderLines = false;
        const bool tableStyleShowHorizontalHeaderUnderline = true;
        const bool tableStyleShowHorizontalScrollIndicators = true;
        const bool tableStyleFullRowSelect = true;
        const bool tableStyleShowVerticalCellLines = false;
        const bool tableStyleExpandLastColumn = true;
        const bool tableStyleSmoothHorizontalScrolling = true;

        private ColorScheme? redColorSchemeAlt;
        private ColorScheme? yellowColorSchemeAlt;
        private ColorScheme? greenColorSchemeAlt;

        public MainView(MainViewModel viewModel) : base()
        {
            ViewModel = viewModel;
        }
        private ColorScheme CreateColorScheme(Color color)
        {
            var colorScheme = new ColorScheme()
            {
                //Disabled = View.ColorScheme.Disabled,
                HotFocus = Application.Driver.MakeAttribute(color, View.ColorScheme.HotFocus.Background),
                Focus = Application.Driver.MakeAttribute(color, Color.Black),
                Normal = Application.Driver.MakeAttribute(color, Color.Black),
                HotNormal = Application.Driver.MakeAttribute(color, View.ColorScheme.HotFocus.Background),
            };
            return colorScheme;
        }
        public override void Setup()
        {
            var top = Application.Top;

            redColorSchemeAlt = new ColorScheme()
            {
                Disabled = View.ColorScheme.Disabled,
                HotFocus = View.ColorScheme.HotFocus,
                Focus = View.ColorScheme.Focus,
                Normal = Application.Driver.MakeAttribute(Color.BrightRed, Color.Blue),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightRed, Color.Blue),
            };

            yellowColorSchemeAlt = new ColorScheme()
            {
                Disabled = View.ColorScheme.Disabled,
                HotFocus = View.ColorScheme.HotFocus,
                Focus = View.ColorScheme.Focus,
                Normal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Blue),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Blue)
            };

            greenColorSchemeAlt = new ColorScheme()
            {
                Disabled = View.ColorScheme.Disabled,
                HotFocus = View.ColorScheme.HotFocus,
                Focus = View.ColorScheme.Focus,
                Normal = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Blue),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Blue)
            };

            View.ColorScheme = Styles.ColorView;

            SetupMenuHeader();
            SetupStatusBar();

            var instanceTable = InstanceTable();

            var workerServiceTable = ServiceTable();
            var gatewayRouteServiceTable = GatewayTable();
            var loadBalancerServiceTable = LoadBalancerTable();
            var messageQueueServiceTable = MessageQueueTable();
            var taskSchedulerServiceTable = TaskSchedulerTable();
            var serverTable = ComputerTable();
            var clientTable = ComputerTable();
            var profilerTable = ProfilerTable();

            var loggerView = LogView();

            var tabItems = new TabItem[] {
                new TabItem("Services (0)", workerServiceTable),                // 0
                new TabItem("Gateway routes (0)", gatewayRouteServiceTable),    // 1
                new TabItem("Load balancers (0)", loadBalancerServiceTable),    // 2
                new TabItem("MQ (0)", messageQueueServiceTable),                // 3
                new TabItem("MQ Scheduler (0)", profilerTable),                 // 4
                new TabItem("Servers (0)", serverTable),                        // 5
                new TabItem("Clients (0)", clientTable),                        // 6
                new TabItem("Threads (0)", profilerTable),                      // 7
                new TabItem("Loggers (0)", loggerView)                          // 8
            };
            var tabView = TabView(instanceTable, tabItems);
            var tabViewTabs = tabView.Tabs.ToArray();

            workerServiceTable.UpdateView();
            gatewayRouteServiceTable.UpdateView();
            serverTable.UpdateView();
            clientTable.UpdateView();
            profilerTable.UpdateView();

            loggerView.UpdateView();

            SetupScrollBar(instanceTable);
            //SetupScrollBar(table2);
            //SetupScrollBar(table3);
            //SetupScrollBar(logView);

            #region Table 3
            workerServiceTable.SetColumnStyle(x => x.ServiceUID!, p => p.RepresentationGetter = (v) => new Guid(v.ToString()!).GetLastBlock());
            workerServiceTable.SetColumnStyle(x => x.ServiceLocation!, p => p.RepresentationGetter = (v) => PathExtention.GetAbbreviatedPath(v?.ToString()!));
            workerServiceTable.SetColumnStyle(x => x.ServiceTrafficLimit, p => p.Alignment = TextAlignment.Right);
            workerServiceTable.SetColumnStyle(x => x.ServiceErrorRateLimit, p => p.Alignment = TextAlignment.Right);
            workerServiceTable.SetColumnStyle(x => x.ServiceInstanceLimit, p => p.Alignment = TextAlignment.Right);

            workerServiceTable.MouseClick += e =>
            {
                if (e.MouseEvent.Flags != MouseFlags.Button3Clicked) return;

                int x = e.MouseEvent.X;
                int y = e.MouseEvent.Y;

                // Calculate the row index based on the mouse Y coordinate and the TableView's top position
                int rowIndex = y - 2;

                // Ensure the row index is within the bounds of the TableView's source
                if (rowIndex >= 0 && rowIndex < workerServiceTable.Table.Rows.Count)
                {
                    workerServiceTable.SelectedRow = rowIndex;
                    var service = workerServiceTable.GetRow(rowIndex);                 

                    MenuItem[] CreateMenuToInstances()
                    {
                        MenuItem[] menuItems = new MenuItem[] 
                        { 
                            new MenuItem("Start all instances", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.StartAllServiceInstances))),
                            new MenuItem("Stop all instances", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.StopAllServiceInstances))),                            
                        };                        
                        return menuItems.ToArray();
                    }

                    MenuItem[] CreateMenuToDepedences()
                    {
                        MenuItem[] menuItems = new MenuItem[]
                        {
                            new MenuItem("Add Gateway Service", "", () => AddNewGatewayService(service)),
                            new MenuItem("Add Server", "", () => AddNewServerClient(service, isServer: true)),
                            new MenuItem("Add Client", "", () => AddNewServerClient(service, isServer: false)),
                        };
                        return menuItems.ToArray();
                    }

                    MenuItem[] CreateMenuToServers()
                    {                        
                        MenuItem[] menuItems = new MenuItem[]
                        {
                            new MenuItem("Deploy Service In Server", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.StartAllServiceInstances))),
                            new MenuItem("Deploy Service To All Servers", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.StopAllServiceInstances)))
                        };
                        return menuItems.ToArray();
                    }

                    MenuItem[] CreateMenuToServices()
                    {
                        MenuItem[] menuItems = new MenuItem[]
                        {
                            new MenuItem("Edit Service", "", () => EditService(service)),
                            new MenuItem("Remove Service", "", () => RemoveService(service))
                        };
                        return menuItems.ToArray();
                    }

                    Func<MenuItem[]>[] funcs = new Func<MenuItem[]>[]
                    {
                        CreateMenuToInstances,
                        CreateLineToContextMenu,
                        CreateMenuToDepedences,
                        CreateLineToContextMenu,
                        CreateMenuToServers,
                        CreateLineToContextMenu,
                        CreateMenuToServices
                    };

                    ShowHeaderContextMenu(tabView!, e, funcs);
                }
            };
            #endregion

            instanceTable.MouseClick += e =>
            {
                if (e.MouseEvent.Flags != MouseFlags.Button3Clicked) return;

                int x = e.MouseEvent.X;
                int y = e.MouseEvent.Y;

                // Calculate the row index based on the mouse Y coordinate and the TableView's top position
                int rowIndex = y - 2;

                // Ensure the row index is within the bounds of the TableView's source
                if (rowIndex >= 0 && rowIndex < instanceTable.Table.Rows.Count)
                {
                    instanceTable.SelectedRow = rowIndex;
                    var serviceInstance = instanceTable.GetRow(rowIndex);

                    MenuItem[] CreateMenuToServicesInstances()
                    {
                        MenuItem[] menuItems = new MenuItem[]
                        {
                            new MenuItem("Start instance", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstance, ServiceManagement.ActionType.StartServiceInstance))),
                            new MenuItem("Stop instance", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstance, ServiceManagement.ActionType.StopServiceInstance))),
                            new MenuItem("Restart instance", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstance, ServiceManagement.ActionType.RestartServiceInstance))),
                            new MenuItem("Start Profiler", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstance, ServiceManagement.ActionType.StartServiceMonitor))),
                            new MenuItem("Stop Profiler", "", () => ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(serviceInstance, ServiceManagement.ActionType.StopServiceMonitor)))
                        };
                        return menuItems.ToArray();
                    }

                    ShowHeaderContextMenu(null!, e, CreateMenuToServicesInstances);                    
                }
            };

            ViewModel!.MessageQueueServiceInstance.Subscribe(nameof(MainView), (message, action) => 
            {
                switch (message)
                {
                    case WorkerServiceInstanceModel workerServiceInstance:
                        {
                            if (action == MessageAction.None)
                            {
                                instanceTable.AddOrUpdateRow(workerServiceInstance, x => new { x.ProcessName, x.ProcessID, x.LatestSession, x.ServiceState });
                                if (workerServiceInstance.Server != null)
                                {
                                    var workerService = workerServiceTable.GetRow(x => x.ServiceUID == workerServiceInstance.ServiceAgentUID);
                                    if (workerService != null)
                                    {
                                        workerServiceInstance.Server.ServiceAgentUID = workerService.ServiceUID;
                                        workerServiceInstance.Server.ServiceType = ServiceType.Worker;
                                        workerServiceInstance.Server.Color = workerService.Color;
                                        workerServiceInstance.Server.IsServer = true;

                                        serverTable.AddOrUpdateRow(workerServiceInstance.Server!);
                                        tabViewTabs[5].Text = $"Servers ({serverTable.Count})";
                                    }
                                }
                            }
                            else if (action == MessageAction.Remove)
                            {
                                instanceTable.RemoveRow(workerServiceInstance);
                            }
                        }
                        break;
                    case GatewayRoutingServiceModel gatewayRoutingService:
                        {
                            if (action == MessageAction.None)
                            {
                                var workerService = workerServiceTable.GetRow(x => x.ServiceUID == gatewayRoutingService.ServiceAgentUID);
                                if (workerService != null)
                                {
                                    gatewayRouteServiceTable.AddOrUpdateRow(gatewayRoutingService, x => new { x.ProcessName, x.ProcessID, x.LatestSession, x.ServiceState });
                                    gatewayRoutingService.Color = workerService.Color;

                                    if (gatewayRoutingService.Server != null)
                                    {                                        
                                            gatewayRoutingService.Server.ServiceAgentUID = workerService.ServiceUID;
                                            gatewayRoutingService.Server.ServiceType = ServiceType.GatewayRoute;
                                            gatewayRoutingService.Server.Color = workerService.Color;
                                            gatewayRoutingService.Server.IsServer = true;

                                            serverTable.AddOrUpdateRow(gatewayRoutingService.Server!);
                                            tabViewTabs[5].Text = $"Servers ({serverTable.Count})";                                        
                                    }
                                }
                            }
                            else if (action == MessageAction.Remove)
                            {
                                gatewayRouteServiceTable.RemoveRow(gatewayRoutingService);
                            }

                            tabViewTabs[1].Text = $"Gateway routes ({gatewayRouteServiceTable.Count})";
                        }
                        break;
                }                
            });


            ViewModel.MessageQueueService.Subscribe(nameof(MainView), message => 
            {                
                workerServiceTable.AddOrUpdateRow(message);
                tabViewTabs[0].Text = $"Services ({workerServiceTable.Count})";
            });

            ViewModel.MessageQueueGatewayService.Subscribe(nameof(MainView), message =>
            {
                gatewayRouteServiceTable.AddOrUpdateRow(message);
                tabViewTabs[1].Text = $"Gateway routes ({gatewayRouteServiceTable.Count})";
            });

            ViewModel.MessageQueueLoadBalancer.Subscribe(nameof(MainView), message =>
            {
                loadBalancerServiceTable.AddOrUpdateRow(message);
                tabViewTabs[2].Text = $"Load balancers ({loadBalancerServiceTable.Count})";
            });

            ViewModel.MessageQueueBusService.Subscribe(nameof(MainView), message =>
            {
                messageQueueServiceTable.AddOrUpdateRow(message);
                tabViewTabs[3].Text = $"MQ ({messageQueueServiceTable.Count})";
            });

            ViewModel.MessageQueueTaskSchedulerService.Subscribe(nameof(MainView), message =>
            {
                taskSchedulerServiceTable.AddOrUpdateRow(message);
                tabViewTabs[4].Text = $"MQ Scheduling ({messageQueueServiceTable.Count})";
            });

            ViewModel.MessageQueueComputer.Subscribe(nameof(MainView), message =>
            {
                if (message.IsServer)
                {
                    serverTable.AddOrUpdateRow(message);
                    tabViewTabs[5].Text = $"Servers ({serverTable.Count})";
                }
                if (message.IsClient)
                {
                    clientTable.AddOrUpdateRow(message);
                    tabViewTabs[6].Text = $"Clients ({clientTable.Count})";
                }
            });

            ViewModel.MessageQueueProcessThread.Subscribe(nameof(MainView), message => 
            {
                profilerTable.AddOrUpdateRow(message);
                tabViewTabs[7].Text = $"Threads ({profilerTable.Count})";
            });

            ViewModel.MessageQueueLogger.Subscribe(nameof(MainView), loggerView.AddOrUpdateRow);
        }

        public delegate void AddOrUpdateRow<T>(T processThread);

        public GridView<WorkerServiceInstanceModel> InstanceTable()
        {
            var table1 = new GridView<WorkerServiceInstanceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(50),
                FullRowSelect = true
            };

            table1.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table1.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table1.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table1.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table1.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            //table1.FullRowSelect = tableStyleFullRowSelect;
            table1.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table1.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table1.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;
            View.Add(table1);

            table1.AddColumn(x => x.Color!, " ");
            table1.AddColumn(x => x.ServiceUID!, "UID");
            table1.AddColumn(x => x.ServiceName!, "Service Name");
            table1.AddColumn(x => x.ServiceVersion!, "Version");
            table1.AddColumn(x => x.ServiceState, "Status");
            table1.AddColumn(x => x.ProcessID, "PID");            
            table1.AddColumn(x => x.ServiceAddress!, "API");            
            table1.AddColumn(x => x.BusPackageIn, "Pack In");
            table1.AddColumn(x => x.BusPackageOut, "Pack Out");            
            table1.AddColumn(x => x.LatestSession, "Latest session");

            table1.AddRestriction(x => x.ServiceAddress!, RestrictionType.ID);

            table1.SetColumnStyle(x => x.ServiceVersion!, p => p.Alignment = TextAlignment.Right);
            table1.SetColumnStyle(x => x.ServiceState, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {                    
                    switch ((string)a.CellValue)
                    {
                        case nameof(ServiceState.Running):                            
                            return CreateColorScheme(Color.BrightYellow);                            
                        case nameof(ServiceState.Started):
                            return CreateColorScheme(Color.Green);
                        case nameof(ServiceState.Stopped):
                            return CreateColorScheme(Color.Red);
                        default:
                            return CreateColorScheme(Color.DarkGray);
                    }                    
                };
            });
            //table1.SetColumnStyle(x => x.ServiceLocation, p => p.RepresentationGetter = (v) => PathExtention.GetAbbreviatedPath(v?.ToString()!));
            table1.SetColumnStyle(x => x.ServiceUID!, p => p.RepresentationGetter = (v) => new Guid(v.ToString()!).GetLastBlock());
            table1.SetColumnStyle(x => x.BusPackageIn, p => p.Alignment = TextAlignment.Right);
            table1.SetColumnStyle(x => x.BusPackageOut, p => p.Alignment = TextAlignment.Centered);            
            table1.SetColumnStyle(x => x.LatestSession, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.RepresentationGetter = (v) =>
                {                    
                    DateTime dateTime = DateTime.Parse(v.ToString()!);
                    return dateTime.ToLocalTime().ToString("hh:mm:ss dd/MM");
                };
            });
            table1.SetColumnStyle(x => x.Color!, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {
                    colors.TryGetValue(a.CellValue.ToString()!, out Color color);
                    return CreateColorScheme(color);
                };
                p.RepresentationGetter = (v) => "■";
            });

            table1.UpdateView();

            return table1;
        }

        public TabView TabView(View previous, params TabItem[] views)
        {
            var tabView = new TabView()
            {
                X = 0,
                Y = Pos.Bottom(previous),
                Width = Dim.Fill(),
                Height = Dim.Percent(50)
            };

            foreach (TabItem tabItem in views)
            {
                var tabPage1 = new TabView.Tab(tabItem.Title, tabItem.View);
                tabView.AddTab(tabPage1, true);
            }

            tabView.SelectedTab = tabView.Tabs.First();

            View.Add(tabView);

            return tabView;
        }

        public class TabItem
        {
            public TabItem(string title, View view) {
                Title = title;
                View = view;
            }
            public string? Title { get; set; }
            public View? View { get; set; }
        }

        public GridView<ProcessThreadModel> ProfilerTable()
        {
            var table2 = new GridView<ProcessThreadModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            //View.Add(table2);

            table2.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table2.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table2.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table2.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table2.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            //table2.FullRowSelect = tableStyleFullRowSelect;
            table2.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table2.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table2.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table2.AddColumn(x => x.TaskID, "TaskID");
            table2.AddColumn(x => x.BasePriority, "BasePriority");
            table2.AddColumn(x => x.CurrentPriority, "CurrentPriority");
            table2.AddColumn(x => x.PriorityLevel, "PriorityLevel");
            table2.AddColumn(x => x.StartTime, "StartTime");
            table2.AddColumn(x => x.TotalProcessorTime, "TotalProcessorTime");
            table2.AddColumn(x => x.UserProcessorTime, "UserProcessorTime");
            table2.AddColumn(x => x.PrivilegedProcessorTime, "PrivilegedProcessorTime");
            table2.AddColumn(x => x.ThreadState, "ThreadState");
            table2.AddColumn(x => x.WaitReason, "WaitReason");

            table2.AddRestriction(x => x.TaskID, RestrictionType.ID);

            return table2;
        }

        public GridView<WorkerServiceModel> ServiceTable()
        {
            var table3 = new GridView<WorkerServiceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            table3.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table3.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table3.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table3.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table3.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table3.FullRowSelect = tableStyleFullRowSelect;
            table3.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table3.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table3.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table3.AddColumn(x => x.Color!, " ");
            table3.AddColumn(x => x.ServiceUID!, "UID");
            table3.AddColumn(x => x.ServiceName!, "Service Name");
            table3.AddColumn(x => x.ServiceVersion!, "Version");
            table3.AddColumn(x => x.ServiceAddress!, "Address");
            table3.AddColumn(x => x.ServicePortRange!, "Port Range");
            table3.AddColumn(x => x.GatewayServiceAddress!, "API Gateway");
            table3.AddColumn(x => x.ServiceStartType!, "Start Type");
            table3.AddColumn(x => x.ServiceTrafficLimit, "Traffic");
            table3.AddColumn(x => x.ServiceErrorRateLimit, "Rate");
            table3.AddColumn(x => x.ServiceInstanceLimit, "Instance");
            table3.AddColumn(x => x.ServiceLocation!, "Location");            

            table3.AddRestriction(x => x.ServiceUID!, RestrictionType.ID);

            table3.SetColumnStyle(x => x.Color!, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {
                    colors.TryGetValue(a.CellValue.ToString()!, out Color color);
                    return CreateColorScheme(color);                    
                };                
                p.RepresentationGetter = (v) => "■";
            });

            return table3;
        }
        private Dictionary<string, Color> colors = new Dictionary<string, Color>()
        {
            { "Blue", Color.Blue },
            { "Green", Color.Green },
            { "Cyan", Color.Cyan },
            { "Red", Color.Red },
            { "Magenta", Color.Magenta },
            { "Brown", Color.Brown },
            { "Gray", Color.Gray },
            { "DarkGray", Color.DarkGray },
            { "BrightBlue", Color.BrightBlue },
            { "BrightGreen", Color.BrightGreen },
            { "BrightCyan", Color.BrightCyan },
            { "BrightRed", Color.BrightRed },
            { "BrightMagenta", Color.BrightMagenta },
            { "BrightYellow", Color.BrightYellow },
            { "White", Color.White },
        };

        public GridView<GatewayRoutingServiceModel> GatewayTable()
        {
            var table1 = new GridView<GatewayRoutingServiceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(50),
                FullRowSelect = true
            };

            table1.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table1.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table1.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table1.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table1.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table1.FullRowSelect = tableStyleFullRowSelect;
            table1.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table1.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table1.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;            

            table1.AddColumn(x => x.Color!, " ");
            table1.AddColumn(x => x.ServiceUID!, "UID");
            table1.AddColumn(x => x.ServiceName!, "Service Name");
            table1.AddColumn(x => x.ServiceVersion!, "Version");
            table1.AddColumn(x => x.ServiceAddress!, "Address");
            table1.AddColumn(x => x.ServiceState, "Status");
            table1.AddColumn(x => x.ProcessID, "PID");
            table1.AddColumn(x => x.LatestSession, "Latest session");
            table1.AddColumn(x => x.ServiceLocation!, "Location");            

            table1.AddRestriction(x => x.ServiceAddress!, RestrictionType.ID);            

            table1.SetColumnStyle(x => x.ServiceVersion!, p => p.Alignment = TextAlignment.Right);
            table1.SetColumnStyle(x => x.ServiceState, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {
                    switch ((string)a.CellValue)
                    {
                        case nameof(ServiceState.Running):
                            return CreateColorScheme(Color.BrightYellow);
                        case nameof(ServiceState.Started):
                            return CreateColorScheme(Color.Green);
                        case nameof(ServiceState.Stopped):
                            return CreateColorScheme(Color.Red);
                        default:
                            return CreateColorScheme(Color.DarkGray);
                    }
                };
            });
            table1.SetColumnStyle(x => x.ServiceLocation!, p => p.RepresentationGetter = (v) => PathExtention.GetAbbreviatedPath(v?.ToString()!));
            table1.SetColumnStyle(x => x.ServiceUID!, p => p.RepresentationGetter = (v) => new Guid(v.ToString()!).GetLastBlock());            
            table1.SetColumnStyle(x => x.LatestSession, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.RepresentationGetter = (v) =>
                {
                    DateTime dateTime = DateTime.Parse(v.ToString()!);
                    return dateTime.ToLocalTime().ToString("hh:mm:ss dd/MM");
                };
            });
            table1.SetColumnStyle(x => x.Color!, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {
                    colors.TryGetValue(a.CellValue.ToString()!, out Color color);
                    return CreateColorScheme(color);
                };
                p.RepresentationGetter = (v) => "■";
            });

            return table1;
        }

        public GridView<LoadBalancerServiceModel> LoadBalancerTable()
        {
            var table3 = new GridView<LoadBalancerServiceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            table3.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table3.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table3.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table3.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table3.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table3.FullRowSelect = tableStyleFullRowSelect;
            table3.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table3.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table3.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table3.AddColumn(x => x.ServiceUID!, "UID");
            table3.AddColumn(x => x.ServiceName!, "Proxy Name");
            table3.AddColumn(x => x.ServiceVersion!, "Version");
            table3.AddColumn(x => x.ServiceAddress!, "Address");
            table3.AddColumn(x => x.ServiceAgentUID!, "Agent UID");
            //table3.AddColumn(x => x.ServiceLocation!, "Location");
            table3.AddColumn(x => x.ProcessName!, "Proccess Name", visible: false);
            table3.AddColumn(x => x.ProcessID, "PID");
            table3.AddColumn(x => x.ServiceState!, "Status");

            table3.AddRestriction(x => x.ServiceUID!, RestrictionType.ID);

            return table3;
        }

        public GridView<MessageQueueServiceModel> MessageQueueTable()
        {
            var table3 = new GridView<MessageQueueServiceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            table3.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table3.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table3.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table3.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table3.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table3.FullRowSelect = tableStyleFullRowSelect;
            table3.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table3.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table3.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table3.AddColumn(x => x.UID!, "UID");
            table3.AddColumn(x => x.Name!, "Name");
            table3.AddColumn(x => x.SubscriptorsCount!, "Subscriptors");
            
            table3.AddRestriction(x => x.UID!, RestrictionType.ID);

            return table3;
        }
        public GridView<TaskSchedulerServiceModel> TaskSchedulerTable()
        {
            var table3 = new GridView<TaskSchedulerServiceModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            table3.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table3.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table3.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table3.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table3.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table3.FullRowSelect = tableStyleFullRowSelect;
            table3.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table3.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table3.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table3.AddColumn(x => x.TaskUID!, "UID");
            table3.AddColumn(x => x.TaskName!, "Name");
            table3.AddColumn(x => x.ServiceUID!, "Service UID");
            table3.AddColumn(x => x.TaskExecutionDate!, "Execution Date");            
            table3.AddColumn(x => x.TaskHasExpiration!, "Has Expiration");
            table3.AddColumn(x => x.TaskExpirationDate!, "Expiration Date");
            table3.AddColumn(x => x.TaskStatus!, "Status");

            table3.AddRestriction(x => x.TaskUID!, RestrictionType.ID);

            return table3;
        }
        public GridView<ServerModel> ComputerTable()
        {
            var table3 = new GridView<ServerModel>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            table3.Style.AlwaysShowHeaders = tableStyleAlwaysShowHeaders;
            table3.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            table3.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            table3.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            table3.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            table3.FullRowSelect = tableStyleFullRowSelect;
            table3.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            table3.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            table3.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            table3.AddColumn(x => x.Color!, " ");
            table3.AddColumn(x => x.ServiceAgentUID!, "Agent UID");
            table3.AddColumn(x => x.ServiceType!, "Service");
            table3.AddColumn(x => x.HostName!, "HostName");
            table3.AddColumn(x => x.IpGeoLocation!.Ip!, "IP Public");
            table3.AddColumnExpression(x => string.Concat(x.IpGeoLocation!.City!, "-", x.IpGeoLocation.CountryName), "Region");
            //table3.AddColumn(x => x.UserName!, "UserName");
            table3.AddColumn(x => x.OS!, "OS");
            table3.AddColumn(x => x.OSArquitecture!, "OS Arch.");
            table3.AddColumn(x => x.Processors!, "Processors");
            table3.AddColumn(x => x.ProcessArquitecture!, "Process Arch.");
            table3.AddColumn(x => x.RAM!, "RAM");
            table3.AddColumn(x => x.NetworkAddresses![0].MAC!, "MAC");
            table3.AddColumn(x => x.NetworkAddresses![0].IPv4!, "IPv4");
            table3.AddColumn(x => x.NetworkAddresses![0].IPv6!, "IPv6");
            table3.AddColumn(x => x.NetworkAddresses![0].Speed!, "Speed");

            table3.AddRestriction(x => x.ServiceAgentUID!, RestrictionType.ID);
            table3.AddRestriction(x => x.ServiceType!, RestrictionType.ID);
            table3.AddRestriction(x => x.HostName!, RestrictionType.ID);

            
            table3.SetColumnStyle(x => x.ServiceAgentUID!, p => p.RepresentationGetter = (v) => new Guid(v.ToString()!).GetLastBlock());            
            table3.SetColumnStyle(x => x.Color!, p =>
            {
                p.Alignment = TextAlignment.Centered;
                p.ColorGetter = (a) =>
                {
                    colors.TryGetValue(a.CellValue.ToString()!, out Color color);
                    return CreateColorScheme(color);
                };
                p.RepresentationGetter = (v) => "■";
            });

            return table3;
        }

        public LoggerView LogView()
        {
            var log = new LoggerView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            log.Style.AlwaysShowHeaders = !tableStyleAlwaysShowHeaders;
            log.Style.ShowHorizontalHeaderOverline = tableStyleShowHorizontalHeaderOverline;
            log.Style.ShowVerticalHeaderLines = tableStyleShowVerticalHeaderLines;
            log.Style.ShowHorizontalHeaderUnderline = tableStyleShowHorizontalHeaderUnderline;
            log.Style.ShowHorizontalScrollIndicators = tableStyleShowHorizontalScrollIndicators;
            log.FullRowSelect = tableStyleFullRowSelect;
            log.Style.ShowVerticalCellLines = tableStyleShowVerticalCellLines;
            log.Style.ExpandLastColumn = tableStyleExpandLastColumn;
            log.Style.SmoothHorizontalScrolling = tableStyleSmoothHorizontalScrolling;

            log.AddColumn(x => x.Message!);
            log.AddRestriction(x => x.Row, RestrictionType.ID);

            return log;
        }
        #region Server and Client
        void AddNewServerClient(WorkerServiceModel serviceModel, bool isServer = false)
        { 

        }
        void EditServerClient(WorkerServiceModel serviceModel)
        {

        }
        void RemoveServerClient(WorkerServiceModel serviceModel)
        {

        }
        #endregion

        #region Gateway service
        void AddNewGatewayService(WorkerServiceModel serviceModel)
        {
            var viewModel = ViewModel!.ServiceProvider.GetService<IRegisterGatewayServiceViewModel>() as RegisterGatewayServiceViewModel;
            viewModel!.AgentServiceUID = serviceModel.ServiceUID;
            viewModel!.ServiceAddress = serviceModel.GatewayServiceAddress;
            viewModel.Color = serviceModel.Color;

            var registerGatewayServiceDialog = new RegisterGatewayServiceDialog(viewModel!);

            Application.Run(registerGatewayServiceDialog);
        }
        void EditGatewayService(GatewayRoutingServiceModel service)
        {
            var viewModel = ViewModel!.ServiceProvider.GetService<IRegisterGatewayServiceViewModel>();
            viewModel!.LoadService(service.ServiceUID!);

            var registerGatewayServiceDialog = new RegisterGatewayServiceDialog(viewModel!);

            Application.Run(registerGatewayServiceDialog);
        }
        void RemoveGatewayService(GatewayRoutingServiceModel service)
        {
            ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.RemoveService));
        }
        #endregion

        #region Service
        void AddNewService()
        {
            var viewModel = ViewModel!.ServiceProvider.GetService<IRegisterServiceViewModel>() as RegisterServiceViewModel;
            var registerServiceDialog = new RegisterServiceDialog(viewModel!);

            Application.Run(registerServiceDialog);
        }
        void EditService(WorkerServiceModel service)
        {
            var viewModel = ViewModel!.ServiceProvider.GetService<IRegisterServiceViewModel>();                        
            viewModel!.LoadService(service.ServiceUID!);
            
            var registerServiceDialog = new RegisterServiceDialog(viewModel!);

            Application.Run(registerServiceDialog);
        }
        void RemoveService(WorkerServiceModel service)
        {
            ViewModel!.MessageQueueSMScheduledTask.SendMessage(new ServiceManagement.ScheduledTask(service, ServiceManagement.ActionType.RemoveService));
        }
        #endregion

        bool Quit()
        {
            return true;
        }
        void SetupMenuHeader()
        {
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_Services", new MenuItem[] {
                    new MenuItem("_New", "Register new service", AddNewService),
                    new MenuItem("_Close", "", () => RequestStop()),
                    new MenuItem("_Quit", "", () => { if (Quit()) Application.Top.Running = false; })
                }),
                new MenuBarItem("Vistas", new MenuItem[] {
                    new MenuItem("_Services", "", null),
                    new MenuItem("_Instances", "", null)
                })
            });

            Application.Top.Add(menu);
        }
        void SetupStatusBar()
        {
            StatusBar statusBar = new StatusBar()
            {
                Visible = true,
            };

            string osDescription = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : RuntimeInformation.OSDescription;
            StatusItem OS = new StatusItem(Key.CharMask, $"OS: {osDescription}", null);

            statusBar.Items = new StatusItem[] {
                    new StatusItem(Key.F1, "VegaExpress ® LiikSoft I.E.R.L.", null),
                    OS
                };

            Application.Top.Add(statusBar);
        }
        void SetupScrollBar(GridView tableView)
        {
            TableView tableView1 = new TableView();
            var scrollBar = new ScrollBarView(tableView, true);

            scrollBar.ChangedPosition += () => {
                tableView.RowOffset = scrollBar.Position;
                if (tableView.RowOffset != scrollBar.Position)
                {
                    scrollBar.Position = tableView.RowOffset;
                }
                Application.MainLoop.Invoke(tableView.SetNeedsDisplay);
            };

            tableView.DrawContent += (e) => {
                scrollBar.Size = tableView.Table?.Rows?.Count ?? 0;
                scrollBar.Position = tableView.RowOffset;
                scrollBar.Refresh();
            };
        }

        private MenuItem[] CreateLineToContextMenu() => new MenuItem[] { null! };
        private void ShowHeaderContextMenu(View container, View.MouseEventArgs e, params Func<MenuItem[]>[] actions)
        {
            List<MenuItem> menuItems = new List<MenuItem>();            

            foreach (Func<MenuItem[]> action in actions)
            {
                menuItems.Add(action.Invoke());
            }      

            var contextMenu = new ContextMenu(e.MouseEvent.X + 1, e.MouseEvent.Y + (container == null ? 2 : container.Frame.Y + 5),
                new MenuBarItem(menuItems.ToArray())
            );
            contextMenu.Show();
        }

        MenuItem[] CreateDisabledEnabledMouseItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            MenuItem miIsMouseDisabled = new MenuItem();
            miIsMouseDisabled.Title = "_Disable Mouse";
            miIsMouseDisabled.Shortcut = Key.CtrlMask | Key.AltMask | (Key)miIsMouseDisabled.Title.ToString()!.Substring(1, 1)[0];
            miIsMouseDisabled.CheckType |= MenuItemCheckStyle.Checked;
            miIsMouseDisabled.Action += () => {
                miIsMouseDisabled.Checked = Application.IsMouseDisabled = !miIsMouseDisabled.Checked;
            };
            menuItems.Add(miIsMouseDisabled);

            return menuItems.ToArray();
        }

        public MainViewModel? ViewModel { get; set; }  
        
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (MainViewModel)value;
        }        
        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);            
            GC.SuppressFinalize(this);
        }
    }
}
