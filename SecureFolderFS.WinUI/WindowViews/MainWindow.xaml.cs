using System;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.Views;
using WinRT;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.WindowViews
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private WindowsSystemDispatcherQueueHelper? _systemDispatcherQueueHelper;
        private MicaController? _micaController;
        private SystemBackdropConfiguration? _systemBackdropConfiguration;
        private bool _isMicaSupported;

        public static MainWindow? Instance { get; private set; }

        public IntPtr Hwnd { get; private set; }

        public AppWindow? AppWindow { get; private set; }

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            EnsureEarlyWindow();
        }

        private void EnsureEarlyWindow()
        {
            // Get AppWindow
            Hwnd = WindowNative.GetWindowHandle(this);
            var mainWindowWndId = Win32Interop.GetWindowIdFromWindow(Hwnd);
            AppWindow = AppWindow.GetFromWindowId(mainWindowWndId);

            // Set title
            AppWindow.Title = "SecureFolderFS";

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                // Extend title bar
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                // Set window buttons background to transparent
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                this.ExtendsContentIntoTitleBar = true;
                SetTitleBar(HostPage.CustomTitleBar);
                //HostPage.CustomTitleBar.Margin = new Thickness(0, 0, 138, 0); // Don't cover up Window buttons
            }

            // Set mica material
            _ = TrySetMicaBackdrop();

            // Register ThemeHelper
            var themeHelper = ThemeHelper.RegisterWindowInstance(AppWindow);
            themeHelper.UpdateTheme();

            // Register ThemeHelper callback (for theme change to update the backdrop)
            if (MicaController.IsSupported())
            {
                themeHelper.RegisterForThemeChangedCallback(nameof(MainWindow), _ =>
                {
                    if (_systemBackdropConfiguration is not null)
                    {
                        SetBackdropConfiguration(_systemBackdropConfiguration);
                    }

                    if (!_isMicaSupported)
                    {
                        (Content as MainWindowHostPage)!.Background = (Brush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                    }
                });
            }

            // Set min size
            // TODO: Set min size
        }

        private bool TrySetMicaBackdrop()
        {
            if (_isMicaSupported = MicaController.IsSupported())
            {
                _systemBackdropConfiguration = new();
                _systemDispatcherQueueHelper = new();
                _micaController = new();

                _systemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hook up the policy object
                Activated += MainWindow_Activated;
                Closed += MainWindow_Closed;

                // Enable the backdrop
                var compositionSupportsSystemBackdrop = this.As<ICompositionSupportsSystemBackdrop>();
                _micaController.AddSystemBackdropTarget(compositionSupportsSystemBackdrop);
                _micaController.SetSystemBackdropConfiguration(_systemBackdropConfiguration);

                // Initialize state
                SetBackdropConfiguration(_systemBackdropConfiguration);

                return true;
            }

            (Content as MainWindowHostPage)!.Background = (Brush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"];

            return false; // Mica not supported
        }

        private void SetBackdropConfiguration(SystemBackdropConfiguration systemBackdropConfiguration)
        {
            systemBackdropConfiguration.Theme = ((FrameworkElement)this.Content).ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                ElementTheme.Default => SystemBackdropTheme.Default,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_systemBackdropConfiguration is not null)
            {
                _systemBackdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to use this closed window.
            _micaController?.Dispose();
            _micaController = null;
            _systemBackdropConfiguration = null;
            ThemeHelper.ThemeHelpers[AppWindow!].UnregisterForThemeChangedCallback(nameof(MainWindow));

            this.Activated -= MainWindow_Activated;
        }
    }
}
