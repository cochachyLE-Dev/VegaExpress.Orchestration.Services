using NStack;
using ReactiveUI;

using DynamicData;
using System.Reactive.Disposables;
using ReactiveMarbles.ObservableEvents;

using Terminal.Gui;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Utilities;
using System.Reactive.Linq;
using VegaExpress.Agent.Data.Constants;
using VegaExpress.Agent.Shared;
using VegaExpress.Agent.Constants;

namespace VegaExpress.Agent.Views.Template
{
    public class LanguageSelectionFrame : ScenarioFrame, IViewFor<LanguageSelectionFrameModel>
    {     
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public LanguageSelectionFrame(LanguageSelectionFrameModel viewModel) : base()
        {
            ViewModel = viewModel;
        }

        public override void Setup()
        {
            var title = TitleLabel();
            var listView = ListView(title);
            LoginProgressLabel(listView);

            ContinueButton();
            PreviousButton();
        }

        Label TitleLabel()
        {            
            var label = new Label()
            {
                Y = 1,
                ColorScheme = new ColorScheme() { Normal = Terminal.Gui.Attribute.Make(Color.Blue, Color.Gray) },
                Text = "Preferred language"
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

            string[] languages = Countries.Languages[GlobalData.CountryCode!].ToArray();
            string[] languageCodes = Countries.LanguageCodes[GlobalData.CountryCode!].ToArray();

            listView.SetSource(languages);

            ViewModel.WhenAnyValue(x => x.LanguageCode)
            .Subscribe(LanguageCode =>
            {
                if (LanguageCode != null)
                {
                    listView.SelectedItem = languageCodes.IndexOf(LanguageCode);
                }
            })
            .DisposeWith(_disposable);

            listView.Events().SelectedItemChanged
                .Select(_ => languageCodes[listView.SelectedItem])
                .DistinctUntilChanged()
                .BindTo(ViewModel, x => x.LanguageCode)
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

        Button PreviousButton()
        {
            var previousButton = new Button("_Previous");
            previousButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Previous)
                .DisposeWith(_disposable);

            AddButton(previousButton);
            return previousButton;
        }
        
        Label LoginProgressLabel(View previous)
        {
            var progress = ustring.Make("Loading...");
            var idle = ustring.Make("Select language to continue.");
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

        public LanguageSelectionFrameModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (LanguageSelectionFrameModel)value;
        }

        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
