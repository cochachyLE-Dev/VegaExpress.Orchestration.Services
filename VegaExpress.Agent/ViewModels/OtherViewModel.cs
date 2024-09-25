using ReactiveUI;

using System.Runtime.Serialization;

namespace VegaExpress.Agent.ViewModels
{
    [DataContract]
    public class OtherViewModel : ReactiveObject, IRoutableViewModel
    {
        [IgnoreDataMember]
        public string? UrlPathSegment { get; } = "Other";
        [IgnoreDataMember]
        private readonly IScreen hostScreen;

        public OtherViewModel(IScreen hostScreen)
        {
            this.hostScreen = hostScreen;
        }

        [IgnoreDataMember]
        public IScreen HostScreen => hostScreen;
    }
}
