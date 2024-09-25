using System.Reactive.Subjects;

namespace VegaExpress.Agent.Utilities
{
    public class RouteState
    {
        public Subject<IRoutable> Subject { get; set; } = new Subject<IRoutable>();
        public Action<IRoutable> NavigateTo { get; set; } = null!;
        public Func<Action<IRoutable>, IDisposable> Subscribe { get; set; } = null!;
        public Func<Action<IRoutable>, IDisposable> BeforeSubcribe { get; set; } = null!;        
    }
    public class NavigateState 
    {
        public NavigateState(MainRouteState? mainRouteState, BranchRouteState? branchRouteState)
        {
            MainRouteState = mainRouteState;
            BranchRouteState = branchRouteState;
        }

        public MainRouteState? MainRouteState { get; set; }
        public BranchRouteState? BranchRouteState { get; set; }
    }
    public class MainRouteState: RouteState { }
    public class BranchRouteState : RouteState { }
}