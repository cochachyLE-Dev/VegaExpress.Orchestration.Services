using Terminal.Gui;

namespace VegaExpress.Agent.Utilities
{
    public interface IRoutable : IDisposable { }
    public class ScenarioFrame : IRoutable 
    {
        private bool _disposedValue;
        public List<Button>? Buttons { get; private set; } = new List<Button>();
        public FrameView View { get; set; } = null!;
        public virtual void Init(ColorScheme colorScheme)
        {
            View = new FrameView($"{GetType().Name}")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = colorScheme
            };
        }
        public virtual void Setup() { }
        public void AddButton(Button button)
        {
            Buttons!.Add(button);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }    
    public class ScenarioView : IRoutable
    {
        private bool _disposedValue;
        public Window View { get; set; } = null!;
        public virtual void Init(ColorScheme colorScheme)
        {
            Application.Init();
            View = new Window()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = colorScheme
            };
            Application.Top.Add(View);
        }
        public virtual void Setup() { }
        public virtual void Run()
        {            
            Application.Run();
        }        
        public virtual void RequestStop()
        {
            Application.RequestStop();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {            
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }    
}
