using NStack;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveMarbles.ObservableEvents;

using Splat;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Terminal.Gui;

using VegaExpress.Agent.ViewModels;
using VegaExpress.Agent.Utilities;
using static Terminal.Gui.Dialog;
using VegaExpress.Agent.ViewModels.Template;
using VegaExpress.Agent.Views.Template;
using VegaExpress.Agent.Constants;

namespace VegaExpress.Agent.Views
{    
    public class WelcomeView : ScenarioView, IViewFor<WelcomeViewModel>
    {        
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        internal List<Button> buttons = new List<Button>();
        internal Label? title;                

        public WelcomeView(WelcomeViewModel viewModel)
        {            
            ViewModel = viewModel;            
        }

        public override void Setup()
        {
            Application.Top.LayoutSubviews();

            string termsAndConditions = Properties.Resources.TermsAndConditions;
            Container(ustring.Empty, termsAndConditions.ToString(), 59, 25, ContinueButton(), CancelButton());

            ViewModel!.FrameHost.RouteState.BeforeSubcribe((element) =>
            {
                if (element != null)
                {
                    var routableFrame = element as ScenarioFrame;
                    var frame = routableFrame!.View;

                    View!.Remove(frame);

                    foreach (Button button in routableFrame.Buttons!)
                    {
                        buttons.Remove(button);
                        View.Remove(button);
                        button.Dispose();
                    }
                }
            });

            ViewModel.FrameHost.RouteState.Subscribe(element =>
            {
                var frame = element as ScenarioFrame;
                if (frame != null)
                {
                    frame.Init(new ColorScheme() { Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray) });
                    frame.View.Y = Pos.Bottom(this.title) - 2;
                    frame.View.X = 1;
                    frame.View.Width = Dim.Fill();
                    frame.View.Height = Dim.Fill() - 2;
                    frame.View.Border = new Border() { BorderThickness = new Thickness(0) };
                    frame.Setup();
                    View!.Add(frame.View);

                    foreach (Button button in frame.Buttons!)
                    {
                        buttons.Add(button);
                        View.Add(button);
                    }

                    LayoutStartedHandler();
                }
            });

            var countryFrame = ViewModel.ServiceProvider.GetService<CountrySelectionFrameModel>();
            ViewModel.FrameHost.RouteState.NavigateTo(new CountrySelectionFrame(countryFrame!));
        }

        void Container(ustring title, ustring message, int width, int height, params Button[] buttons) => Container(false, width, height, title, message, 0, null!, buttons);
        void Container(bool useErrorColors, int width, int height, ustring title, ustring message, int defaultButton = 0, Border border = null!, params Button[] buttons)
        {
            int defaultWidth = 50;
            if (defaultWidth > Application.Driver.Cols / 2)
            {
                defaultWidth = (int)(Application.Driver.Cols * 0.60f);
            }
            int maxWidthLine = TextFormatter.MaxWidthLine(message);
            if (maxWidthLine > Application.Driver.Cols)
            {
                maxWidthLine = Application.Driver.Cols;
            }
            if (width == 0)
            {
                maxWidthLine = Math.Max(maxWidthLine, defaultWidth);
            }
            else
            {
                maxWidthLine = width;
            }
            int textWidth = Math.Min(TextFormatter.MaxWidth(message, maxWidthLine), Application.Driver.Cols);
            int textHeight = TextFormatter.MaxLines(message, textWidth); // message.Count (ustring.Make ('\n')) + 1;
            int msgboxHeight = Math.Min(Math.Max(1, textHeight) + 4, Application.Driver.Rows); // textHeight + (top + top padding + buttons + bottom)

            // Create button array for Dialog
            int count = 0;            
            if (buttons != null && defaultButton > buttons.Length - 1)
            {
                defaultButton = buttons.Length - 1;
            }
            foreach (var s in buttons!)
            {                
                if (count == defaultButton)
                {
                    s.IsDefault = true;
                }                
                count++;
            }
                        
            if (width == 0 & height == 0)
            {
                Dialog(title, buttons);
                base.View!.Height = msgboxHeight; 
            }
            else
            {
                Dialog(title, width, Math.Max(height, 4), buttons);
            }

            if (border != null)
            {
                base.View!.Border = border;
            }

            if (useErrorColors)
            {
                base.View!.ColorScheme = Colors.Error;
            }

            if (message != null)
            {
                var msg = new StringBuilder();
                msg.AppendLine(@" ___   __              ____                          ");
                msg.AppendLine(@" __ | / /__ ___ ____ _/ __/_ __ ___  _______ ___ ___®");
                msg.AppendLine(@" __ |/ / -_) _ `/ _ `/ _/ \ \ // _ \/ __/ -_|_-<(_-< ");
                msg.AppendLine(@" _____/\__/\_, /\_,_/___//_\_\/ .__/_/  \__/___/___/ ");
                msg.AppendLine(@"          /___/              /_/   Liiksoft I.E.R.L. ");
                msg.AppendLine(@"");

                var messageStr = msg.ToString();

                int maxWidthLine1 = TextFormatter.MaxWidthLine(messageStr);
                int textWidth1 = Math.Min(TextFormatter.MaxWidth(messageStr, maxWidthLine1), Application.Driver.Cols);
                int textHeight1 = TextFormatter.MaxLines(messageStr, textWidth1);

                this.title = new Label()
                {
                    X = Pos.Center(),
                    Y = 1,
                    Width = textWidth1,
                    Height = textHeight1,
                    Text = messageStr                    
                };
                base.View!.Add(this.title);

                //foreach (var page in pages!)
                //{
                //    page.Y = Pos.Bottom(this.title) - 2;
                //}

                //var termsView = pages![0];                
                //Win.Add(termsView);
            }

            if (width == 0 & height == 0)
            {
                // Dynamically size Width
                base.View!.Width = Math.Min(Math.Max(maxWidthLine, Math.Max(title.ConsoleWidth, Math.Max(textWidth + 2, GetButtonsWidth() + buttons.Count() + 2))), Application.Driver.Cols); // textWidth + (left + padding + padding + right)
            }                     
        }        
        void Dialog(ustring title, params Button[] buttons) => Dialog(title: title, width: 0, height: 0, buttons: buttons);
        void Dialog(ustring title, int width, int height, params Button[] buttons)
        {          
            base.View!.Title = title;

            base.View.X = Pos.Center();
            base.View.Y = Pos.Center();

            if (width == 0 & height == 0)
            {
                base.View.Width = Dim.Percent(85);
                base.View.Height = Dim.Percent(85);
            }
            else
            {
                base.View.Width = width;
                base.View.Height = height;
            }

            base.View.ColorScheme = Styles.ColorBase;
            base.View.Modal = true;
            base.View.Border.Effect3D = false;           

            base.View.LayoutStarted += (args) =>
            {
                LayoutStartedHandler();
            };
        }

        ButtonAlignments ButtonAlignment = ButtonAlignments.Center;

        void LayoutStartedHandler()
        {
            if (buttons.Count == 0 || !base.View.IsInitialized) return;

            int shiftLeft = 0;

            int buttonsWidth = GetButtonsWidth();
            switch (ButtonAlignment)
            {
                case ButtonAlignments.Center:
                    // Center Buttons
                    shiftLeft = (base.View.Bounds.Width - buttonsWidth - buttons.Count - 2) / 2 + 1;
                    for (int i = buttons.Count - 1; i >= 0; i--)
                    {
                        Button button = buttons[i];
                        shiftLeft += button.Frame.Width + (i == buttons.Count - 1 ? 0 : 1);
                        if (shiftLeft > -1)
                        {
                            button.X = Pos.AnchorEnd(shiftLeft);
                        }
                        else
                        {
                            button.X = base.View.Frame.Width - shiftLeft;
                        }
                        button.Y = Pos.AnchorEnd(1);
                    }
                    break;

                case ButtonAlignments.Justify:
                    // Justify Buttons
                    // leftmost and rightmost buttons are hard against edges. The rest are evenly spaced.

                    var spacing = (int)Math.Ceiling((double)(base.View.Bounds.Width - buttonsWidth - (base.View.Border.DrawMarginFrame ? 2 : 0)) / (buttons.Count - 1));
                    for (int i = buttons.Count - 1; i >= 0; i--)
                    {
                        Button button = buttons[i];
                        if (i == buttons.Count - 1)
                        {
                            shiftLeft += button.Frame.Width;
                            button.X = Pos.AnchorEnd(shiftLeft);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                // first (leftmost) button - always hard flush left
                                var left = base.View.Bounds.Width - ((base.View.Border.DrawMarginFrame ? 2 : 0) + base.View.Border.BorderThickness.Left + base.View.Border.BorderThickness.Right);
                                button.X = Pos.AnchorEnd(Math.Max(left, 0));
                            }
                            else
                            {
                                shiftLeft += button.Frame.Width + (spacing);
                                button.X = Pos.AnchorEnd(shiftLeft);
                            }
                        }
                        button.Y = Pos.AnchorEnd(1);
                    }
                    break;

                case ButtonAlignments.Left:
                    // Left Align Buttons
                    var prevButton = buttons[0];
                    prevButton.X = 0;
                    prevButton.Y = Pos.AnchorEnd(1);
                    for (int i = 1; i < buttons.Count; i++)
                    {
                        Button button = buttons[i];
                        button.X = Pos.Right(prevButton) + 1;
                        button.Y = Pos.AnchorEnd(1);
                        prevButton = button;
                    }
                    break;

                case ButtonAlignments.Right:
                    // Right align buttons
                    shiftLeft = buttons[buttons.Count - 1].Frame.Width;
                    buttons[buttons.Count - 1].X = Pos.AnchorEnd(shiftLeft);
                    buttons[buttons.Count - 1].Y = Pos.AnchorEnd(1);
                    for (int i = buttons.Count - 2; i >= 0; i--)
                    {
                        Button button = buttons[i];
                        shiftLeft += button.Frame.Width + 1;
                        button.X = Pos.AnchorEnd(shiftLeft);
                        button.Y = Pos.AnchorEnd(1);
                    }
                    break;
            }
        }        
        
        Button ContinueButton()
        {
            var loginButton = new Button("Continu_e");

            loginButton
                .Events()
            .Clicked
                .InvokeCommand(ViewModel, x => x.Continue)
                .DisposeWith(_disposable);

            //base.View!.Add(loginButton);
            return loginButton;
        }

        Button CancelButton()
        {
            var clearButton = new Button("_Cancel");

            clearButton
                .Events()
                .Clicked
                .InvokeCommand(ViewModel, x => x.Cancel)
                .DisposeWith(_disposable);

            //base.View!.Add(clearButton);
            return clearButton;
        }
        Label LoginProgressLabel(View previous)
        {
            var progress = ustring.Make("Loading in...");
            var idle = ustring.Make("By continuing you accept the terms and conditions.");
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
            base.View!.Add(loginProgressLabel);
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
        
        public WelcomeViewModel? ViewModel { get; set; }        
        object IViewFor.ViewModel
        {
            get => ViewModel!;
            set => ViewModel = (WelcomeViewModel)value;
        }
        protected override void Dispose(bool disposing)
        {
            _disposable.Dispose();
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
