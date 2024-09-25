using ReactiveUI;

using System;
using System.Reactive.Linq;

namespace VegaExpress.Agent.Utilities
{
    public interface IRouter<T> where T : IRoutable
    {
        RouteState RouteState { get; set; }
    }    
    public class Router<T> : ReactiveObject, IRouter<T> where T : IRoutable
    {                        
        private DisposableAction<IRoutable>? _beforeSubcribe;
        public virtual RouteState RouteState { get; set; } = null!;
        public T? CurrentView { get; protected set; }

        public Router(RouteState routeState) 
        {
            RouteState = routeState;                        
            RouteState.BeforeSubcribe = (func) => _beforeSubcribe = new DisposableAction<IRoutable>(func);                        

            RouteState.NavigateTo = NavigateTo;
            RouteState.Subscribe = Subscribe;
        }

        private void NavigateTo(IRoutable message)
        {                        
            RouteState!.Subject!.OnNext(message);            
        }

        private IDisposable Subscribe(Action<IRoutable> handler)
        {
            return RouteState!.Subject!.Subscribe(message =>
            {                                
                _beforeSubcribe?.Invoke(CurrentView!);
                CurrentView = (T)message!;
                handler((T)message!);                
            });
        }
    }
    public class DisposableAction<T> : IDisposable
    {
        private Action<T>? _action1;
        protected DisposableAction() { }
        public DisposableAction(Action<T> action)
        {
            _action1 = action ?? throw new ArgumentNullException(nameof(action));
        }
        public void Invoke(T obj) => _action1?.Invoke(obj);
        public void Dispose()
        {
            _action1 = null;
        }
    }
    public class DisposableAction<T1,T2> : DisposableAction<T1>
    {        
        private Action<T1,T2>? _action2;
        protected DisposableAction() { }
        public DisposableAction(Action<T1> action) : base(action) { }
        public DisposableAction(Action<T1, T2> action):base()
        {
            _action2 = action ?? throw new ArgumentNullException(nameof(action));
        }        
        public void Invoke(T1 obj1, T2 obj2) => _action2?.Invoke(obj1, obj2);
        public new void Dispose()
        {
            _action2 = null;
            base.Dispose();
        }
    }
    public class DisposableFunc<T, TResult> : IDisposable
    {
        private Func<T, TResult>? _func;

        public DisposableFunc(Func<T, TResult> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TResult Invoke(T obj)
        {            
            if(_func != null)
                return _func!.Invoke(obj);
            else
                return default!;
        }

        public void Dispose()
        {
            _func = null;
        }
    }    
}