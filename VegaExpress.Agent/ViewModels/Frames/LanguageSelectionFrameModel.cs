using ReactiveUI;
using ReactiveUI.Fody.Helpers;

//using ReactiveUI.Fody.Helpers;

using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.Views.Template;

namespace VegaExpress.Agent.ViewModels.Template
{    
    public interface ILanguageSelectionFrameModel
    {        
        string? LanguageCode { get; set; }
        ReactiveCommand<Unit, Unit>? Continue { get; }
        ReactiveCommand<Unit, Unit>? Previous { get; }
    }
    [DataContract]
    public class LanguageSelectionFrameModel : Router<ScenarioFrame>, ILanguageSelectionFrameModel
    {        
        [Reactive]
        public string? LanguageCode { get; set; } = GlobalData.LanguageCode!;

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Continue { get; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Previous { get; }

        public LanguageSelectionFrameModel(IServiceProvider serviceProvider, NavigateState navigateState):base(navigateState.BranchRouteState!)
        {                        
            this.WhenAnyValue(x => x.LanguageCode!).Subscribe(value => GlobalData.LanguageCode = value);

            Continue = ReactiveCommand.Create(() =>
            {
                var termsFrame = serviceProvider.GetService<TermsFrameModel>();
                RouteState.NavigateTo(new TermsFrame(termsFrame!));
            });
            Previous = ReactiveCommand.Create(() =>
            {
                var termsFrame = serviceProvider.GetService<CountrySelectionFrameModel>();
                RouteState.NavigateTo(new CountrySelectionFrame(termsFrame!));
            });
        }
    }
}