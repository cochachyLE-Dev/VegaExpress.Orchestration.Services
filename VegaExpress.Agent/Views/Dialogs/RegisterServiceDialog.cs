using NStack;

using ReactiveUI;
using ReactiveMarbles.ObservableEvents;

using System.Reactive.Disposables;

using Terminal.Gui;

using VegaExpress.Agent.ViewModels.Dialog;
using System.Reactive.Linq;
using VegaExpress.Agent.Extentions;
using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Models;
using System.Threading.Tasks;
using VegaExpress.Agent.Services;

namespace VegaExpress.Agent.Views.Dialogs
{
    internal class RegisterServiceDialog : Dialog, IViewFor<IRegisterServiceViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();

        internal List<Button> buttons = new List<Button>();
        internal Label? title;

        public RegisterServiceDialog(IRegisterServiceViewModel viewModel):base("Register Service")
        {
            ViewModel = viewModel;
            Setup();
        }        

        public void Setup()
        {
            base.Width = 57;
            base.Height = 29;
            Application.Top.LayoutSubviews();

            var serviceUIDLabel = ServiceUIDLabel();
            var serviceUIDTextField = ServiceUIDTextField(serviceUIDLabel);
            var serviceNameLabel = ServiceNameLabel(serviceUIDLabel);
            var serviceNameTextField = ServiceNameTextField(serviceNameLabel);
            var serviceVersionLabel = ServiceVersionLabel(serviceNameLabel);
            var serviceVersionTextField = ServiceVersionTextField(serviceVersionLabel);
            var serviceAddressLabel = ServiceAddressLabel(serviceVersionLabel);
            var serviceAddressTextField = ServiceAddressTextField(serviceAddressLabel);
            var servicePortRangeLabel = ServicePortRangeLabel(serviceAddressLabel);
            var servicePortRangeTextField = ServicePortRangeTextField(servicePortRangeLabel);
            var gatewayServiceAddressLabel = GatewayServiceAddressLabel(servicePortRangeLabel);
            var gatewayServiceAddressTextField = GatewayServiceAddressTextField(gatewayServiceAddressLabel);
            var serviceTrafficLabel = ServiceTrafficLabel(gatewayServiceAddressLabel);
            var serviceTrafficTextField = ServiceTrafficTextField(serviceTrafficLabel);
            var serviceErrorRateLabel = ServiceErrorRateLabel(serviceTrafficLabel);
            var serviceErrorRateTextField = ServiceErrorRateTextField(serviceErrorRateLabel);
            var serviceInstancesLabel = ServiceInstancesLabel(serviceErrorRateLabel);
            var serviceInstancesTextField = ServiceInstancesTextField(serviceInstancesLabel);
            var serviceLocationLabel = ServiceLocationLabel(serviceInstancesLabel);
            var serviceLocationTextField = ServiceLocationTextField(serviceLocationLabel);
            var serviceLocationButton = ServiceLocationButton(serviceLocationTextField);
            var serviceStatusLabel = ServiceStatusLabel(serviceLocationLabel);
            var serviceStatusComboBox = ServiceStatusComboBox(serviceStatusLabel);
            var serviceColorLabel = ServiceColorLabel(serviceStatusLabel);
            var serviceColorComboBox = ServiceColorComboBox(serviceColorLabel);
            var saveProgressLabel = SaveProgressLabel(serviceColorLabel);

            var saveButton = SaveButton();
            var cancelButton = CancelButton();
        }        

        Label ServiceUIDLabel()
        {
            var serviceUIDLbl = new Label("UID:")
            {
                X = 1,
                Y = 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceUIDLbl);
            return serviceUIDLbl;
        }

        TextField ServiceUIDTextField(View previous)
        {
            var serviceUIDTxt = new TextField("")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceUID)
                .Select(value => (ustring)value)
                .BindTo(serviceUIDTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceUIDTxt
                .Events()
                .TextChanged
                .Select(old => serviceUIDTxt.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceUID)
                .DisposeWith(_disposable);
            base.Add(serviceUIDTxt);
            return serviceUIDTxt;
        }

        Label ServiceNameLabel(View previous)
        {
            var serviceNameLbl = new Label("Name:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceNameLbl);
            return serviceNameLbl;
        }

        TextField ServiceNameTextField(View previous)
        {
            var serviceNameTxt = new TextField("")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceName)
                .Select(value => (ustring)value)
                .BindTo(serviceNameTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceNameTxt
                .Events()
                .TextChanged
                .Select(old => serviceNameTxt.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceName)
                .DisposeWith(_disposable);
            base.Add(serviceNameTxt);
            return serviceNameTxt;
        }

        Label ServiceVersionLabel(View previous)
        {
            var serviceVersionLbl = new Label("Version:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceVersionLbl);
            return serviceVersionLbl;
        }

        TextField ServiceVersionTextField(View previous)
        {
            var serviceVersionTxt = new TextField("1.0.0.0")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceVersion)
                .Select(value => (ustring)value)
                .BindTo(serviceVersionTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceVersionTxt
                .Events()
                .TextChanged
                .Select(old => serviceVersionTxt.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceVersion)
                .DisposeWith(_disposable);
            base.Add(serviceVersionTxt);
            return serviceVersionTxt;
        }

        Label ServiceAddressLabel(View previous)
        {
            var serviceAddressLbl = new Label("Address:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceAddressLbl);
            return serviceAddressLbl;
        }

        TextField ServiceAddressTextField(View previous)
        {
            var serviceAddressTxt = new TextField("https://127.0.0.1")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceAddress)
                .Select(value => (ustring)value)
                .BindTo(serviceAddressTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceAddressTxt
                .Events()
                .TextChanged
                .Select(old => serviceAddressTxt.Text.ToString())
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceAddress)
                .DisposeWith(_disposable);
            base.Add(serviceAddressTxt);
            return serviceAddressTxt;
        }

        Label ServicePortRangeLabel(View previous)
        {
            var servicePortRangeLbl = new Label("Port Range (,-):")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(servicePortRangeLbl);
            return servicePortRangeLbl;
        }        

        TextField ServicePortRangeTextField(View previous)
        {
            var servicePortRangeTxt = new TextField("8080-8090")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServicePortRange)
                .Select(value => (ustring)value)
                .BindTo(servicePortRangeTxt, x => x.Text)
                .DisposeWith(_disposable);
            servicePortRangeTxt
                .Events()
                .TextChanged
                .Select(old => servicePortRangeTxt.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServicePortRange)
                .DisposeWith(_disposable);
            base.Add(servicePortRangeTxt);
            return servicePortRangeTxt;
        }

        Label GatewayServiceAddressLabel(View previous)
        {
            var serviceAddressLbl = new Label("Gateway Address:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceAddressLbl);
            return serviceAddressLbl;
        }

        TextField GatewayServiceAddressTextField(View previous)
        {
            var serviceAddressTxt = new TextField("https://127.0.0.1:8080")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.GatewayServiceAddress)
                .Select(value => (ustring)value)
                .BindTo(serviceAddressTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceAddressTxt
                .Events()
                .TextChanged
                .Select(old => serviceAddressTxt.Text.ToString())
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.GatewayServiceAddress)
                .DisposeWith(_disposable);
            base.Add(serviceAddressTxt);
            return serviceAddressTxt;
        }

        Label ServiceTrafficLabel(View previous)
        {
            var serviceTrafficLbl = new Label("Traffic Limit:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceTrafficLbl);
            return serviceTrafficLbl;
        }

        TextField ServiceTrafficTextField(View previous)
        {
            var serviceTrafficTxt = new TextField("1")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceTrafficLimit)
                .Select(value => ustring.Make(value.ToString()))
                .BindTo(serviceTrafficTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceTrafficTxt
                .Events()
                .TextChanged
                .Select(old => int.Parse(serviceTrafficTxt.Text.ToString()!))
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceTrafficLimit)
                .DisposeWith(_disposable);
            base.Add(serviceTrafficTxt);
            return serviceTrafficTxt;
        }

        Label ServiceErrorRateLabel(View previous)
        {
            var serviceErrorRateLbl = new Label("Error Rate Limit:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceErrorRateLbl);
            return serviceErrorRateLbl;
        }

        TextField ServiceErrorRateTextField(View previous)
        {
            var serviceErrorRateTxt = new TextField("3")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceErrorRateLimit)
                .Select(value => ustring.Make(value.ToString()))
                .BindTo(serviceErrorRateTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceErrorRateTxt
                .Events()
                .TextChanged
                .Select(old => int.Parse(serviceErrorRateTxt.Text.ToString()!))
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceErrorRateLimit)
                .DisposeWith(_disposable);
            base.Add(serviceErrorRateTxt);
            return serviceErrorRateTxt;
        }

        Label ServiceInstancesLabel(View previous)
        {
            var serviceInstancesLbl = new Label("Instance Limit:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };            
            base.Add(serviceInstancesLbl);
            return serviceInstancesLbl;
        }

        TextField ServiceInstancesTextField(View previous)
        {
            var serviceInstancesTxt = new TextField("5")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34
            };
            ViewModel
                .WhenAnyValue(x => x.ServiceInstanceLimit)
                .Select(value => ustring.Make(value.ToString()))
                .BindTo(serviceInstancesTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceInstancesTxt
                .Events()
                .TextChanged
                .Select(old => int.Parse(serviceInstancesTxt.Text.ToString()!))
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceInstanceLimit)
                .DisposeWith(_disposable);
            base.Add(serviceInstancesTxt);
            return serviceInstancesTxt;
        }

        Label ServiceLocationLabel(View previous)
        {
            var serviceLocationLbl = new Label("Location:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };            
            base.Add(serviceLocationLbl);
            return serviceLocationLbl;
        }

        TextField ServiceLocationTextField(View previous)
        {
            var serviceLocationTxt = new TextField("")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 27
            };            
            ViewModel
                .WhenAnyValue(x => x.ServiceLocation)
                .Select(value => (ustring)value)
                //.Select(PathExtention.GetAbbreviatedPath!)
                .BindTo(serviceLocationTxt, x => x.Text)
                .DisposeWith(_disposable);
            serviceLocationTxt
                        .Events()
                        .TextChanged
                        .Select(old => serviceLocationTxt.Text)
                        .DistinctUntilChanged()
                        .BindTo(ViewModel, x => x.ServiceLocation)
                        .DisposeWith(_disposable);
            base.Add(serviceLocationTxt);
            return serviceLocationTxt;
        }

        Button ServiceLocationButton(TextField serviceLocationTxt)
        {
            var serviceLocationBtn = new Button("...")
            {
                X = Pos.Right(serviceLocationTxt),
                Y = Pos.Top(serviceLocationTxt),
                Width = 2
            };

            string filePath = string.Empty;
            serviceLocationBtn.Clicked += () =>
            {
                var openDialog = new OpenDialog("Open File", "Open a file")
                {
                    AllowsMultipleSelection = true
                };

                Application.Run(openDialog);

                if (!string.IsNullOrEmpty(openDialog.FilePath.ToString()))
                {
                    filePath = openDialog.FilePath.ToString()!;
                    serviceLocationTxt.Text = filePath;
                }
            };
            base.Add(serviceLocationBtn);
            return serviceLocationBtn;
        }

        Label ServiceStatusLabel(View previous)
        {
            var serviceStatusLbl = new Label("Status:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceStatusLbl);
            return serviceStatusLbl;
        }

        ComboBox ServiceStatusComboBox(View previous)
        {
            var serviceStatusCbx = new ComboBox()
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34,
                Height = Dim.Fill(2),
                HideDropdownListOnClick = true
            };

            List<string> enumList = Enum.GetNames(typeof(ServiceStartType)).ToList();            

            serviceStatusCbx.SetSource(enumList);

            ViewModel
                .WhenAnyValue(x => x.ServiceStartType)
                .Select(value => (ustring)value)
                .BindTo(serviceStatusCbx, x => x.SearchText)
                .DisposeWith(_disposable);
            serviceStatusCbx
                .Events()
                .SelectedItemChanged
                .Select(old => serviceStatusCbx.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.ServiceStartType)
                .DisposeWith(_disposable);
            base.Add(serviceStatusCbx);
            return serviceStatusCbx;
        }

        Label ServiceColorLabel(View previous)
        {
            var serviceStatusLbl = new Label("Color:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 17,
                AutoSize = true,
                TextAlignment = TextAlignment.Right
            };
            base.Add(serviceStatusLbl);
            return serviceStatusLbl;
        }

        ComboBox ServiceColorComboBox(View previous)
        {
            var serviceColorCbx = new ComboBox()
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 34,
                Height = Dim.Fill(2),
                HideDropdownListOnClick = true
            };

            List<string> enumList = Enum.GetNames(typeof(Color)).ToList();

            serviceColorCbx.SetSource(enumList);

            ViewModel
                .WhenAnyValue(x => x.Color)
                .Select(value => (ustring)value)
                .BindTo(serviceColorCbx, x => x.SearchText)
                .DisposeWith(_disposable);
            serviceColorCbx
                .Events()
                .SelectedItemChanged
                .Select(old => serviceColorCbx.Text)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.Color)
                .DisposeWith(_disposable);
            base.Add(serviceColorCbx);
            return serviceColorCbx;
        }

        Button SaveButton()
        {
            var saveButton = new Button("_Save");

            saveButton
                .Events()
            .Clicked
                .InvokeCommand(ViewModel, x => x.Save)                
                .DisposeWith(_disposable);

            ViewModel!.Save!.ThrownExceptions.Subscribe(ex => 
            {
                var messageError = "Por favor, asegúrese de llenar todos los campos obligatorios.";
                MessageBox.Query("Message", messageError, "_Ok");
            });

            ViewModel!.Save!.Subscribe(_ => Application.RequestStop(this));

            base.AddButton(saveButton);

            return saveButton;
        }

        Button CancelButton()
        {
            var cancelButton = new Button("_Cancel");

            cancelButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Cancel)
                .DisposeWith(_disposable);

            ViewModel!.Cancel!.Subscribe(_ => Application.RequestStop(this));
            
            base.AddButton(cancelButton);

            return cancelButton;
        }
        Label SaveProgressLabel(View previous)
        {
            var progress = ustring.Make("Saving...");
            var loginProgressLabel = new Label()
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = 40
            };
            ViewModel
                .WhenAnyObservable(x => x.Save!.IsExecuting)
                .Select(executing => executing ? progress : "")
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(loginProgressLabel, x => x.Text)
                .DisposeWith(_disposable);

            return loginProgressLabel;
        }
        internal int GetButtonsWidth()
        {
            if (buttons.Count == 0)
            {
                return 0;
            }
            return buttons.Select(b => b.Bounds.Width).Sum();
        }

        public IRegisterServiceViewModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (IRegisterServiceViewModel)value;
        }        

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
