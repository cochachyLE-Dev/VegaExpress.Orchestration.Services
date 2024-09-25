using NStack;
using ReactiveUI;

using System.Reactive.Disposables;
using ReactiveMarbles.ObservableEvents;

using Terminal.Gui;
using System.Reactive.Linq;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Utilities;

namespace VegaExpress.Agent.Views.Template
{
    public class TermsFrame : ScenarioFrame, IViewFor<TermsFrameModel>
    {
        private readonly CompositeDisposable _disposable = new CompositeDisposable();        

        public TermsFrame(TermsFrameModel viewModel) : base()
        {
            ViewModel = viewModel;            
        }
        public override void Setup()
        {
            var label1 = Label1();
            var textView = TextView(label1);
            var label2 = Label2(textView);            
            LoginProgressLabel(label2);

            ContinueButton();
            PreviousButton();            
        }
        TextView TextView(View previous)
        {            
            ScrollBarView scrollBar;
            TextView textView = new TextView()
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = Dim.Fill() - 1,
                Height = Dim.Fill() - 3,
                BottomOffset = 1,
                RightOffset = 1,
                ReadOnly = true,
                ColorScheme = colorScheme
            };

            string termsAndConditions = Properties.Resources.TermsAndConditions;
            textView.Text = termsAndConditions;

            View.Add(textView);

            scrollBar = new ScrollBarView(textView, true);
            scrollBar.ChangedPosition += () => {
                textView.TopRow = scrollBar.Position;
                if (textView.TopRow != scrollBar.Position)
                {
                    scrollBar.Position = textView.TopRow;
                }
                textView.SetNeedsDisplay();
            };

            scrollBar.OtherScrollBarView.ChangedPosition += () => {
                textView.LeftColumn = scrollBar.OtherScrollBarView.Position;
                if (textView.LeftColumn != scrollBar.OtherScrollBarView.Position)
                {
                    scrollBar.OtherScrollBarView.Position = textView.LeftColumn;
                }
                textView.SetNeedsDisplay();
            };

            scrollBar.VisibleChanged += () => {
                if (scrollBar.Visible && textView.RightOffset == 0)
                {
                    textView.RightOffset = 1;
                }
                else if (!scrollBar.Visible && textView.RightOffset == 1)
                {
                    textView.RightOffset = 0;
                }
            };

            scrollBar.OtherScrollBarView.VisibleChanged += () => {
                if (scrollBar.OtherScrollBarView.Visible && textView.BottomOffset == 0)
                {
                    textView.BottomOffset = 1;
                }
                else if (!scrollBar.OtherScrollBarView.Visible && textView.BottomOffset == 1)
                {
                    textView.BottomOffset = 0;
                }
            };

            textView.DrawContent += (e) => {
                scrollBar.Size = textView.Lines;
                scrollBar.Position = textView.TopRow;
                if (scrollBar.OtherScrollBarView != null)
                {
                    scrollBar.OtherScrollBarView.Size = textView.Maxlength + 1;
                    scrollBar.OtherScrollBarView.Position = textView.LeftColumn;
                }
                scrollBar.LayoutSubviews();
                scrollBar.Refresh();
            };
            return textView;
        }
        
        private readonly ColorScheme colorScheme = new ColorScheme()
        {
            Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            HotNormal = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            HotFocus = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray)
        };

        Label Label1()
        {
            var label = new Label()
            {
                Y = 1,
                ColorScheme = new ColorScheme()
                {
                    Normal = Terminal.Gui.Attribute.Make(Color.Blue, Color.Gray)
                },
                Text = "Términos y Condiciones de Uso del Software"
            };
            View.Add(label);
            return label;
        }

        Label Label2(View previous)
        {
            var label = new Label()
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = Dim.Fill() - 1,
                Height = 3
            };
            label.Text = "Al continuar, aceptas los términos y condiciones  de\r\nuso del software.";

            View.Add(label);
            return label;
        }               

        Label LoginProgressLabel(View previous)
        {
            var progress = ustring.Make("Logging in...");
            var idle = ustring.Make("Press 'Login' to log in.");
            var loginProgressLabel = new Label(idle)
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40
            };
            ViewModel
                .WhenAnyObservable(x => x.Continue!.IsExecuting)
                .Select(executing => executing ? progress : idle)
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(loginProgressLabel, x => x.Text)
                .DisposeWith(_disposable);
            View.Add(loginProgressLabel);
            return loginProgressLabel;
        }

        Button ContinueButton()
        {
            var continueButton = new Button("Continu_e");
            continueButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Continue)
                .DisposeWith(_disposable);
            
            AddButton(continueButton);
            return continueButton;
        }

        Button PreviousButton()
        {
            var previousButton = new Button("_Previous");
            previousButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Cancel)
                .DisposeWith(_disposable);

            AddButton(previousButton);
            return previousButton;
        }
        public TermsFrameModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (TermsFrameModel)value;
        }

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
