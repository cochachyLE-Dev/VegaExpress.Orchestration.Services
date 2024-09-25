using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Reactive;
using System.Runtime.Serialization;

using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.Views.Template;

namespace VegaExpress.Agent.ViewModels.Template
{
    public interface ITermsFrameModel
    {        
        ReactiveCommand<Unit, Unit>? Continue { get; }

        ReactiveCommand<Unit, Unit>? Cancel { get; }
    }
    [DataContract]
    public class TermsFrameModel : Router<ScenarioFrame>, ITermsFrameModel
    {        
        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Continue { get; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Cancel { get; }

        public TermsFrameModel(IServiceProvider serviceProvider, NavigateState navigateState) : base(navigateState.BranchRouteState!)
        {                                
            Continue = ReactiveCommand.Create(() =>
            {
                var loginFrame = serviceProvider.GetService<LoginFrameModel>();
                RouteState.NavigateTo(new LoginFrame(loginFrame!));
            });
            Cancel = ReactiveCommand.Create(() =>
            {
                var languageFrame = serviceProvider.GetService<LanguageSelectionFrameModel>();
                RouteState.NavigateTo(new LanguageSelectionFrame(languageFrame!));
            });            
        }
    }
}
