using Microsoft.Extensions.DependencyInjection;

using VegaExpress.Agent.Services;
using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.ViewModels;
using VegaExpress.Agent.ViewModels.Template;

namespace VegaExpress.Agent
{
    public static class RouterBase
    {        
        public static void AddRouteServices(this IServiceCollection services)
        {            
            var mainRouteState = new MainRouteState();            

            var welcomeDependency = new Dependency(services, new NavigateState(mainRouteState, new BranchRouteState()));            
            welcomeDependency.RegisterService<WelcomeViewModel>(
                (s) => s.RegisterDependency<CountrySelectionFrameModel>(),
                (s) => s.RegisterDependency<LanguageSelectionFrameModel>(),
                (s) => s.RegisterDependency<TermsFrameModel>(),
                (s) => s.RegisterDependency<LoginFrameModel>()
            );

            var mainDependency = new Dependency(services, new NavigateState(mainRouteState, new BranchRouteState()));
            mainDependency.RegisterService<MainViewModel>();

            //var registerServiceDependency = new Dependency(services, new NavigateState(mainRouteState, new BranchRouteState()));
            //registerServiceDependency.RegisterService<RegisterServiceDialog>();

            services.AddHostedService<TerminalService>(provider =>
            {
                var serviceProvider = provider.GetRequiredService<IServiceProvider>();
                return new TerminalService(serviceProvider, welcomeDependency.NavigateState);
            });            
        }
    }
    public class Dependency
    {
        private readonly IServiceCollection _services;        
        public readonly NavigateState NavigateState;

        public Dependency(IServiceCollection services, NavigateState navigateState)
        {
            _services = services;
            NavigateState = navigateState;            
        }

        public void RegisterService<T>(params Action<Dependency>[] registerDependencies) where T : class
        {
            foreach (var registerDependency in registerDependencies)
            {
                registerDependency(this);
            }

            Register<T>();
        }

        public void RegisterDependency<T>() where T : class
        {
            Register<T>();
        }

        private void Register<T>() where T : class
        {
            var type = typeof(T);
            var constructor = type.GetConstructors()[0];
            var parameters = constructor.GetParameters();

            _services.AddSingleton<T>(provider =>
            {
                var serviceProvider = provider.GetRequiredService<IServiceProvider>();

                var args = parameters.Select(parameter =>
                {
                    if (parameter.ParameterType == typeof(NavigateState))
                        return Activator.CreateInstance(typeof(NavigateState), NavigateState.MainRouteState, NavigateState.BranchRouteState);
                    else
                        return serviceProvider.GetService(parameter.ParameterType);
                }).ToArray();

                return (T)Activator.CreateInstance(typeof(T), args)!;
            });
        }
    }
}