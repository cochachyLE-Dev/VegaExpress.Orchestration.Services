using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Reactive;
using System.Runtime.Serialization;

using VegaExpress.Agent.Data.Constants;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Utilities;
using VegaExpress.Agent.Views.Template;

namespace VegaExpress.Agent.ViewModels.Template
{
    public interface ICountrySelectionFrameModel
    {        
        string? CountryCode { get; set; }
        ReactiveCommand<Unit, Unit>? Continue { get; }        
        ReactiveCommand<Unit, Unit>? Cancel { get; }
    }
    [DataContract]
    public class CountrySelectionFrameModel : Router<ScenarioFrame>, ICountrySelectionFrameModel
    {        
        [Reactive]
        public string? CountryCode { get; set; } = GlobalData.CountryCode!;

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Continue { get; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit, Unit>? Cancel { get; }

        public CountrySelectionFrameModel(IServiceProvider serviceProvider, NavigateState navigateState) : base(navigateState.BranchRouteState!)
        {                                    
            this.WhenAnyValue(x => x.CountryCode).Subscribe(value =>
            {
                GlobalData.CountryCode = value;
                GlobalData.LanguageCode = null;
            });

            Continue = ReactiveCommand.Create(() =>
            {
                var languageFrame = serviceProvider.GetService<LanguageSelectionFrameModel>();
                RouteState.NavigateTo(new LanguageSelectionFrame(languageFrame!));
            });
            Cancel = ReactiveCommand.Create(() =>
            {
                                
            });            
        }
    }
}