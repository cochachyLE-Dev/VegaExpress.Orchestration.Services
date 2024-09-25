using Splat;
using Terminal.Gui;

using VegaExpress.Agent.ViewModels;
using VegaExpress.Agent.Views;

namespace VegaExpress.Agent.Utilities
{
    public class TerminalService : BackgroundService
    {
        readonly IServiceProvider _serviceProvider;
        readonly NavigateState _navigateState;
        public readonly Router<ScenarioView> Router;

        public TerminalService(IServiceProvider serviceProvider, NavigateState navigateState)
        {
            _serviceProvider = serviceProvider;
            _navigateState = navigateState;
            Router = new Router<ScenarioView>(_navigateState.MainRouteState!);            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            return Task.Run(() =>
            {
                Router.RouteState.BeforeSubcribe((e) =>
                {
                    if (Application.Top != null)
                    {
                        Application.RequestStop();
                        Application.Shutdown();
                    }
                });
                Router.RouteState.Subscribe(scenario =>
                {
                    var view = scenario as ScenarioView;
                    if (view != null)
                    {
                        view.Init(Colors.Base);
                        view.Setup();
                        view.Run();
                        view.Dispose();
                    }
                });
                
                Router.RouteState.NavigateTo(new WelcomeView(new WelcomeViewModel(_serviceProvider, _navigateState)));
            });
        }                
    }
}