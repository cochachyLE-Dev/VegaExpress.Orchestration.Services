using ReactiveUI;

using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;

using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Views.Template;

namespace VegaExpress.Agent.ViewModels
{
    public interface IWelcomeViewModel
    {
        ReactiveCommand<Unit, Unit>? Continue { get; }
        ReactiveCommand<Unit, Unit>? Cancel { get; }
    }
    public class WelcomeViewModel : Router<ScenarioView>, IWelcomeViewModel
    {
        public readonly IServiceProvider ServiceProvider;        
        public readonly Router<ScenarioFrame> FrameHost;

        public WelcomeViewModel(IServiceProvider serviceProvider, NavigateState navigateState) :base(navigateState.MainRouteState!)
        {
            ServiceProvider = serviceProvider;
            FrameHost = new Router<ScenarioFrame>(navigateState.BranchRouteState!);
            //Continue = ReactiveCommand.Create(() =>
            //{
            //    FrameHost.RouteState.NavigateTo(new LoginFrame(new LoginFrameModel(FrameHost.RouteState)));                
            //});

            //Cancel = ReactiveCommand.Create(() =>
            //{
            //    FrameHost.RouteState.NavigateTo(new TermsFrame(new TermsFrameModel(FrameHost.RouteState)));
            //});
        }

        private void Builder()
        {            
            //HostScreen.Router.Navigate.Execute(new LoginViewModel(HostScreen));

        }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Continue { get; protected set; }
        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Cancel { get; protected set; }        

        private IDisposable Initialize() => Disposable.Create(Builder);
    }    
}
