using NStack;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using VegaExpress.Agent.Utilities;
using System.Runtime.Serialization;
using VegaExpress.Agent.Views;
using VegaExpress.Agent.Views.Template;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Generated;

namespace VegaExpress.Agent.ViewModels.Template
{
    public interface ILoginFrameModel 
    {        
        ustring? Username { get; set; }
        
        ustring? Password { get; set; }
        
        ReactiveCommand<Unit, Unit>? Login { get; }
        
        ReactiveCommand<Unit, Unit>? Clear { get; }
        
        ReactiveCommand<Unit, Unit>? Previous { get; }
        
        bool IsValid { get; }
    }    
    [DataContract]
    public class LoginFrameModel : Router<ScenarioFrame>, ILoginFrameModel
    {                        
        [IgnoreDataMember]
        readonly ObservableAsPropertyHelper<bool> _isValid;

        public LoginFrameModel(IServiceProvider serviceProvider, NavigateState navigateState) : base(navigateState.BranchRouteState!)
        {                        
            var canLogin = this.WhenAnyValue(
                x => x.Username,
                x => x.Password,
                (username, password) =>
                    !ustring.IsNullOrEmpty(username) &&
                    !ustring.IsNullOrEmpty(password));

            _isValid = canLogin.ToProperty(this, x => x.IsValid);

            Login = ReactiveCommand.Create(() =>
            {
                var mainViewModel = serviceProvider.GetService<MainViewModel>();                
                navigateState.MainRouteState!.NavigateTo(new MainView(mainViewModel!));
            });

            Previous = ReactiveCommand.Create(() =>
            {
                var termsFrame = serviceProvider.GetService<TermsFrameModel>();
                RouteState.NavigateTo(new TermsFrame(termsFrame!));
            });

            Clear = ReactiveCommand.Create(() => { });
            Clear.Subscribe(unit =>
            {
                Username = ustring.Empty;
                Password = ustring.Empty;
            });
        }

        [Reactive]
        public ustring? Username { get; set; } = ustring.Empty;
        [Reactive]
        public ustring? Password { get; set; } = ustring.Empty;

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Login { get; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Clear { get; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Previous { get; }

        [IgnoreDataMember]
        public bool IsValid => _isValid.Value;        
    }
}
