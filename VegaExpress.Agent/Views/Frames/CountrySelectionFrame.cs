using NStack;
using ReactiveUI;

using System.Reactive.Disposables;
using ReactiveMarbles.ObservableEvents;

using Terminal.Gui;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Utilities;
using System.Reactive.Linq;
using VegaExpress.Agent.Data.Constants;
using DynamicData;
using VegaExpress.Agent.Constants;

namespace VegaExpress.Agent.Views.Template
{
    internal class CountrySelectionFrame : ScenarioFrame, IViewFor<CountrySelectionFrameModel>
    {
        private readonly CompositeDisposable _disposable = new CompositeDisposable();        

        public CountrySelectionFrame(CountrySelectionFrameModel viewModel) : base()
        {
            ViewModel = viewModel;
        }

        public override void Setup()
        {
            var title = TitleLabel();
            var listView = ListView(title);

            LoginProgressLabel(listView);
            
            ContinueButton();
            CancelButton();
        }

        Label TitleLabel()
        {
            var label = new Label()
            {
                Y = 1,
                ColorScheme = new ColorScheme() { Normal = Terminal.Gui.Attribute.Make(Color.Blue, Color.Gray) },
                Text = "Country or region information"
            };
            View.Add(label);
            return label;
        }

        ListView ListView(View previous)
        {
            ColorScheme colorSchemeScrollBar = new ColorScheme()
            {
                Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                HotNormal = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
                HotFocus = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
                Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray)
            };
            ColorScheme colorScheme = new ColorScheme()
            {
                Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                Focus = Terminal.Gui.Attribute.Make(Color.White, Color.Black),
                HotNormal = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
                HotFocus = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
                Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray)
            };

            var listView = new ListView()
            {
                X = Pos.Left(previous),
                Y = Pos.Top(previous) + 2,
                Height = Dim.Fill() - 2,
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false,
                ColorScheme = Styles.ColorBase//colorScheme
            };

            View.Add(listView);

            var scrollBar = new ScrollBarView(listView, true);
            scrollBar.ColorScheme = colorSchemeScrollBar;

            scrollBar.ChangedPosition += () => {
                listView.TopItem = scrollBar.Position;
                if (listView.TopItem != scrollBar.Position)
                {
                    scrollBar.Position = listView.TopItem;
                }
                listView.SetNeedsDisplay();
            };

            scrollBar.OtherScrollBarView.ChangedPosition += () => {
                listView.LeftItem = scrollBar.OtherScrollBarView.Position;
                if (listView.LeftItem != scrollBar.OtherScrollBarView.Position)
                {
                    scrollBar.OtherScrollBarView.Position = listView.LeftItem;
                }
                listView.SetNeedsDisplay();
            };

            listView.DrawContent += (e) => {
                scrollBar.Size = listView.Source.Count;
                scrollBar.Position = listView.TopItem;
                scrollBar.OtherScrollBarView.Size = listView.Maxlength;
                scrollBar.OtherScrollBarView.Position = listView.LeftItem;
                scrollBar.Refresh();
            };

            var countryNames = Countries.Names.Select(item => item.Value).ToArray();
            listView.SetSource(countryNames);

            ViewModel.WhenAnyValue(x => x.CountryCode)
            .Subscribe(countryCode =>
            {
                if (countryCode != null)
                {
                    listView.SelectedItem = countryNames.IndexOf(Countries.Names[countryCode]);                   
                }
            })
            .DisposeWith(_disposable);

            listView.Events().SelectedItemChanged
                .Select(_ => Countries.Names.Single(x => x.Value == countryNames[listView.SelectedItem]).Key)
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.CountryCode)
                .DisposeWith(_disposable);

            return listView;
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

        Button CancelButton()
        {
            var cancelButton = new Button("_Cancel");
            cancelButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Cancel)
                .DisposeWith(_disposable);

            AddButton(cancelButton);
            return cancelButton;
        }

        Label LoginProgressLabel(View previous)
        {
            var progress = ustring.Make("Loading...");
            var idle = ustring.Make("Select country or region to continue.");
            var loginProgressLabel = new Label(idle)
            {
                X = Pos.Left(previous),
                Y = Pos.Bottom(previous) + 1,
                Width = Dim.Fill()
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

        public CountrySelectionFrameModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (CountrySelectionFrameModel)value;
        }

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
