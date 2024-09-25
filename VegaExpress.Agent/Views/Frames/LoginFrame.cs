using NStack;
using ReactiveUI;

using System.Reactive.Disposables;
using ReactiveMarbles.ObservableEvents;

using Terminal.Gui;
using System.Reactive.Linq;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Utilities;
using System.Runtime.Serialization;
using VegaExpress.Agent.Constants;

namespace VegaExpress.Agent.Views.Template
{
    [DataContract]
    public class LoginFrame : ScenarioFrame, IViewFor<LoginFrameModel>
    {
        private readonly CompositeDisposable _disposable = new CompositeDisposable();        

        public LoginFrame(LoginFrameModel viewModel) : base()
        {
            ViewModel = viewModel;
        }

        public override void Setup()
        {
            var panel = Panel();
            var title = TitleLabel(panel);
            var usernameLabel = UsernameLabel(title);
            var usernameInput = UsernameInput(usernameLabel);
            var passwordLabel = PasswordLabel(usernameInput);
            var passwordInput = PasswordInput(passwordLabel);
            var validationLabel = ValidationLabel(passwordInput);
            LoginProgressLabel(validationLabel);

            LoginButton();
            ClearButton();
            PreviousButton();
        }

        PanelView Panel()
        {
            var borderStyle = BorderStyle.Rounded;
            var drawMarginFrame = true;
            var borderThickness = new Thickness(2);
            var borderBrush = Color.Brown;
            var padding = new Thickness(2);
            var background = Color.BrightBlue;
            var effect3D = true;

            var panel = new PanelView()
            {
                X = Pos.Center() - 20,
                Y = Pos.Center(),
                Width = 24,
                Height = 13,
                Border = new Border()
                {
                    BorderStyle = borderStyle,
                    DrawMarginFrame = drawMarginFrame,
                    BorderThickness = borderThickness,
                    BorderBrush = borderBrush,
                    Padding = padding,
                    Background = background,
                    Effect3D = effect3D,
                    Title = "Panel"
                },
                ColorScheme = Colors.TopLevel,
                Text = "Login Form"
            };
            View.Add(panel);
            return panel;
        }

        Label TitleLabel(View previous)
        {
            var label = new Label("Login Form")
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
            };
            View.Add(label);
            return label;
        }

        TextField UsernameInput(View previous)
        {
            var usernameInput = new TextField(ViewModel!.Username)
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40,
                ColorScheme = Styles.ColorBase
            };
            ViewModel
                .WhenAnyValue(x => x.Username)
                .BindTo(usernameInput, x => x.Text)
                .DisposeWith(_disposable);
            usernameInput
                .Events()
                .TextChanged
                .Select(old => usernameInput.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.Username)
                .DisposeWith(_disposable);
            View.Add(usernameInput);
            return usernameInput;
        }

        Label UsernameLabel(View previous)
        {
            var usernameLengthLabel = new Label
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40,
                Text = "Username"
            };
            View.Add(usernameLengthLabel);
            return usernameLengthLabel;
        }

        TextField PasswordInput(View previous)
        {
            var passwordInput = new TextField(ViewModel!.Password)
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40,
                Secret = true,
                ColorScheme = Styles.ColorBase
            };
            ViewModel
                .WhenAnyValue(x => x.Password)
                .BindTo(passwordInput, x => x.Text)
                .DisposeWith(_disposable);
            passwordInput
                .Events()
                .TextChanged
                .Select(old => passwordInput.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.Password)
                .DisposeWith(_disposable);
            View.Add(passwordInput);
            return passwordInput;
        }

        Label PasswordLabel(View previous)
        {
            var passwordLengthLabel = new Label
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40,
                Text = "Password"
            };
            View.Add(passwordLengthLabel);
            return passwordLengthLabel;
        }

        Label ValidationLabel(View previous)
        {
            var error = ustring.Make("Please, enter user name and password.");
            var success = ustring.Make("The input is valid!");
            var validationLabel = new Label(error)
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40
            };
            ViewModel
                .WhenAnyValue(x => x.IsValid)
                .Select(valid => valid ? success : error)
                .BindTo(validationLabel, x => x.Text)
                .DisposeWith(_disposable);
            ViewModel
                .WhenAnyValue(x => x.IsValid)
                .Select(valid => valid ? Styles.ColorBase : Colors.Error)
                .BindTo(validationLabel, x => x.ColorScheme)
                .DisposeWith(_disposable);
            View.Add(validationLabel);

            var focus = Styles.ColorBase.Focus;
            var background = focus.Background;
            var color = focus.Foreground;

            return validationLabel;
        }

        Label LoginProgressLabel(View previous)
        {
            var progress = ustring.Make("Logging in...");
            var idle = ustring.Make("");
            var loginProgressLabel = new Label(idle)
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40,
            };
            ViewModel
                .WhenAnyObservable(x => x.Login!.IsExecuting)
                .Select(executing => executing ? progress : idle)
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(loginProgressLabel, x => x.Text)
                .DisposeWith(_disposable);
            View.Add(loginProgressLabel);

            return loginProgressLabel;
        }

        Button LoginButton()
        {
            var loginButton = new Button("Login");
            loginButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Login)
                .DisposeWith(_disposable);
            
            AddButton(loginButton);
            return loginButton;
        }

        Button ClearButton()
        {
            var clearButton = new Button("Clear");
            clearButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Clear)
                .DisposeWith(_disposable);

            AddButton(clearButton);            
            return clearButton;
        }
        Button PreviousButton()
        {
            var previousButton = new Button("Previous");
            previousButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Previous)
                .DisposeWith(_disposable);

            AddButton(previousButton);
            return previousButton;
        }

        public LoginFrameModel? ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (LoginFrameModel)value;
        }

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
