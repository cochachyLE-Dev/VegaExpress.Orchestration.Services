using NStack;

using ReactiveUI;
using ReactiveMarbles.ObservableEvents;

using System.Reactive.Disposables;

using Terminal.Gui;

using VegaExpress.Agent.ViewModels.Dialog;
using System.Reactive.Linq;
using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Constants;
using VegaExpress.Agent.Data.Models;
using HttpMethod = VegaExpress.Agent.Data.Models.HttpMethod;
using VegaExpress.Agent.Controls;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using System.Reactive.Joins;

namespace VegaExpress.Agent.Views.Dialogs
{    
   internal class RegisterGatewayServiceDialog : Dialog, IViewFor<IRegisterGatewayServiceViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();

        internal List<Button> buttons = new List<Button>();
        internal Label? title;

        public RegisterGatewayServiceDialog(IRegisterGatewayServiceViewModel viewModel) : base("Register Gateway Service")
        {
            ViewModel = viewModel;
            Setup();
        }

        public void Setup()
        {
            base.Width = 65;
            base.Height = 25;
            Application.Top.LayoutSubviews();

            var serviceUIDLabel = ServiceUIDLabel();
            var serviceUIDTextField = ServiceUIDTextField(serviceUIDLabel);
            var serviceNameLabel = ServiceNameLabel(serviceUIDLabel);
            var serviceNameTextField = ServiceNameTextField(serviceNameLabel);
            var serviceVersionLabel = ServiceVersionLabel(serviceNameLabel);
            var serviceVersionTextField = ServiceVersionTextField(serviceVersionLabel);
            var serviceAddressLabel = ServiceAddressLabel(serviceVersionLabel);
            var serviceAddressTextField = ServiceAddressTextField(serviceAddressLabel);
            var serviceLocationLabel = ServiceLocationLabel(serviceAddressLabel);
            var serviceLocationTextField = ServiceLocationTextField(serviceLocationLabel);
            var serviceLocationButton = ServiceLocationButton(serviceLocationTextField);
            var serviceStatusLabel = ServiceStatusLabel(serviceLocationLabel);
            var serviceStatusComboBox = ServiceStatusComboBox(serviceStatusLabel);
            var servicesListLabel = ServicesListLabel(serviceStatusLabel);
            var servicesListView = ServicesListView(servicesListLabel);

            var saveProgressLabel = SaveProgressLabel(servicesListView);

            var saveButton = SaveButton();
            var cancelButton = CancelButton();
        }

        Label ServiceUIDLabel()
        {
            var serviceUIDLbl = new Label("UID:")
            {
                X = 1,
                Y = 1,
                Width = 9,
                AutoSize = true                
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
                Width = 50
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
                Width = 9,
                AutoSize = true                
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
                Width = 50
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
                Width = 9,
                AutoSize = true                
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
                Width = 50
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
                Width = 9,
                AutoSize = true                
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
                Width = 50
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

        Label ServiceLocationLabel(View previous)
        {
            var serviceLocationLbl = new Label("Location:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 9,
                AutoSize = true                
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
                Width = 43
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
                Width = 9,
                AutoSize = true                
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
                Width = 50,
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
        
        Label ServicesListLabel(View previous)
        {
            var serviceStatusLbl = new Label("Routes:")
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = 9,
                AutoSize = true,                
            };
            base.Add(serviceStatusLbl);
            return serviceStatusLbl;
        }
        Label Label(string caption, View previous = null!)
        {
            var label = new Label(caption)
            {
                X = previous == null ? 1 : Pos.Left(previous),
                Y = previous == null ? 1 : Pos.Bottom(previous) + 1,
                Width = 8,
                AutoSize = true                
            };            
            return label;
        }
        GridView ServicesListView(View previous)
        {            
            var listView = new GridView<EndpointRoute>()
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 1,
                Width = Dim.Fill(),
                Height = 7,
                CanFocus = true
            };
            
            listView.Style.AlwaysShowHeaders = true;            
            listView.Style.ShowHorizontalHeaderOverline = false;
            listView.Style.ShowVerticalHeaderLines = false;
            listView.Style.ShowHorizontalHeaderUnderline = false;
            listView.Style.ShowHorizontalScrollIndicators = false;
            listView.FullRowSelect = true;
            listView.Style.ShowVerticalCellLines = false;
            listView.Style.ExpandLastColumn = false;
            listView.Style.SmoothHorizontalScrolling = true;

            List<EndpointRoute> patterns = new List<EndpointRoute>() 
            { 
                new EndpointRoute{ ID = 0, Method = HttpMethod.GET, Pattern = "v1/inventory/track/{*path}", Location = "v1/track/{*path}" },
                new EndpointRoute{ ID = 1, Method = HttpMethod.GET, Pattern = "v1/inventory/order", Location = "v1/order" },
                new EndpointRoute{ ID = 2, Method = HttpMethod.GET, Pattern = "v1/inventory/receive", Location = "v1/receive" },
                new EndpointRoute{ ID = 3, Method = HttpMethod.GET, Pattern = "v1/inventory/storage", Location = "v1/storage" },
                new EndpointRoute{ ID = 4, Method = HttpMethod.GET, Pattern = "v1/inventory/pick", Location = "v1/pick" },
                new EndpointRoute{ ID = 5, Method = HttpMethod.GET, Pattern = "v1/inventory/pack", Location = "v1/pack" },
                new EndpointRoute{ ID = 6, Method = HttpMethod.POST, Pattern = "v1/inventory/ship", Location = "v1/ship" },
                new EndpointRoute{ ID = 7, Method = HttpMethod.PUT, Pattern = "v1/inventory/movement", Location = "v1/movement" }
            };
            
            listView.AddColumn(x => x.Method!, "Method");
            listView.AddColumn(x => x.Pattern!, "Pattern");
            listView.AddColumn(x => x.Location!, "Location");

            listView.AddRestriction(x => x.ID!, RestrictionType.ID);
            listView.AddRestriction(x => x.Method!, RestrictionType.UniqueID);
            listView.AddRestriction(x => x.Pattern!, RestrictionType.UniqueID);

            listView.SetColumnStyle(x => x.Pattern!, p => 
            {
                p.MaxWidth = 25;
                p.MinWidth = 25;
                p.Alignment = TextAlignment.Centered;
                p.RepresentationGetter = (v) => ProcessText(v?.ToString()!, 23);
            });
            listView.SetColumnStyle(x => x.Location!, p =>
            {
                p.MaxWidth = 25;
                p.MinWidth = 25;
                p.Alignment = TextAlignment.Centered;
                p.RepresentationGetter = (v) => ProcessText(v?.ToString()!, 23);
            });

            listView.SetDataSource(patterns);

            string ProcessText(string input, int length = 25)
            {
                if (input.Length <= length)
                {
                    return input;
                }

                string cutText = input[^length..];

                int slashIndex = cutText.IndexOf('/');
                if (slashIndex >= 0)
                {
                    return "..." + cutText[slashIndex..];
                }

                return "..." + cutText;
            }

            base.Add(listView);

            var scrollBar = SetupScrollBar(listView);
            
            var serviceAddBtn = new Button("Add")
            {
                X = Pos.Right(previous) + 2,
                Y = Pos.Top(previous),
                Width = 2
            };

            serviceAddBtn.Clicked += () => ShowDialog(serviceAddBtn.Text);
            base.Add(serviceAddBtn);

            var serviceEditBtn = new Button("Edit")
            {
                X = Pos.Right(serviceAddBtn),
                Y = Pos.Top(serviceAddBtn),
                Width = 2
            };

            serviceEditBtn.Clicked += () =>
            {
                if (listView.SelectedRow == -1) return;
                ShowDialog(serviceEditBtn.Text, true);
            };
            base.Add(serviceEditBtn);

            var serviceRemoveBtn = new Button("Remove")
            {
                X = Pos.Right(serviceEditBtn),
                Y = Pos.Top(serviceEditBtn),
                Width = 2
            };

            serviceRemoveBtn.Clicked += () => 
            {
                if (listView.SelectedRow == -1) return;
                var item = listView[listView.SelectedRow];
                patterns.Remove(item);

                listView.SetDataSource(patterns);

                scrollBar.Size = listView.Count;
                scrollBar.Position = listView.RowOffset;
                scrollBar.Refresh();
            };
            base.Add(serviceRemoveBtn);

            return listView;
            void ShowDialog(ustring title, bool edit = false)            
            {
                var editDialog = new Dialog(title, 45, 11);

                var httpMethodLbl = new Label("Method:")
                {
                    X = 1,
                    Y = 1,
                    Width = 17,
                    AutoSize = true
                };

                var httpMethodCbx = new ComboBox()
                {
                    X = Pos.Left(httpMethodLbl),
                    Y = Pos.Bottom(httpMethodLbl),
                    Width = 40,
                    Height = 5,
                    HideDropdownListOnClick = true
                };

                List<string> httpMethodEnum = Enum.GetNames(typeof(HttpMethod)).ToList();
                httpMethodCbx.SetSource(httpMethodEnum);

                var patternLbl = new Label("Pattern:")
                {
                    X = 1,
                    Y = 3,
                    Width = 17,
                    AutoSize = true
                };
                var patternTextField = new TextField("")
                {
                    X = Pos.Left(patternLbl),
                    Y = Pos.Bottom(patternLbl),
                    Width = 40
                };

                var locationLbl = new Label("Location:")
                {
                    X = 1,
                    Y = 5,
                    Width = 17,
                    AutoSize = true
                }; ;
                var locationTextField = new TextField("")
                {
                    X = Pos.Left(locationLbl),
                    Y = Pos.Bottom(locationLbl),
                    Width = 40
                };

                var saveEndpointButton = new Button("_Save", true);
                var cancelEndpointButton = new Button("_Cancel", true);
                
                saveEndpointButton.Clicked += () => 
                {
                    var httpMethodStr = httpMethodEnum[httpMethodCbx.SelectedItem];
                    HttpMethod httpMethod = (HttpMethod)Enum.Parse(typeof(HttpMethod), httpMethodStr);

                    if (edit)
                    {
                        var endpointRoute = listView[listView.SelectedRow];
                        endpointRoute.Method = httpMethod;
                        endpointRoute.Pattern = patternTextField.Text.ToString()!;
                        endpointRoute.Location = locationTextField.Text.ToString()!;

                        listView.UpdateRow(endpointRoute);                        
                    }
                    else
                    {
                        var endpointRoute = new EndpointRoute { ID = listView.Count > 0 ? listView.Max(x => x.ID) + 1 : 0 };
                        endpointRoute.Method = httpMethod;
                        endpointRoute.Pattern = patternTextField.Text.ToString()!;
                        endpointRoute.Location = locationTextField.Text.ToString()!;

                        listView.AddRow(endpointRoute);
                        listView.SelectedRow = listView.Count - 1;
                    }

                    Application.RequestStop(editDialog);
                };
                cancelEndpointButton.Clicked += () => Application.RequestStop(editDialog);

                editDialog.AddButton(saveEndpointButton);
                editDialog.AddButton(cancelEndpointButton);

                editDialog.Add(httpMethodLbl);
                editDialog.Add(httpMethodCbx);
                editDialog.Add(patternLbl);
                editDialog.Add(patternTextField);
                editDialog.Add(locationLbl);
                editDialog.Add(locationTextField);

                if (edit)
                {
                    var endpointRoute = listView[listView.SelectedRow];
                    if (endpointRoute != null)
                    {
                        httpMethodCbx.SearchText = endpointRoute.Method.ToString()!;
                        patternTextField.Text = (ustring)endpointRoute.Pattern!.ToString();
                        locationTextField.Text = (ustring)endpointRoute.Location!.ToString();
                    }
                }
                else
                {
                    httpMethodCbx.SelectedItem = (int)HttpMethod.GET;
                    patternTextField.Text = "";
                    locationTextField.Text = "";
                }

                Application.Run(editDialog);
            }
        }
        ScrollBarView SetupScrollBar(GridView tableView)
        {            
            var scrollBar = new ScrollBarView(tableView, true);

            scrollBar.ChangedPosition += () => {
                tableView.RowOffset = scrollBar.Position;
                if (tableView.RowOffset != scrollBar.Position)
                {
                    scrollBar.Position = tableView.RowOffset;
                }
                Application.MainLoop.Invoke(tableView.SetNeedsDisplay);
            };

            tableView.DrawContent += (e) => {
                scrollBar.Size = tableView.Table?.Rows?.Count + 1 ?? 0;
                scrollBar.Position = tableView.RowOffset;
                scrollBar.Refresh();
            };

            return scrollBar;
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

        public IRegisterGatewayServiceViewModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (IRegisterGatewayServiceViewModel)value;
        }

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
