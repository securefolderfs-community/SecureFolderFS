using System;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using SecureFolderFS.WinUI.Helpers;
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
                HostPage.CustomTitleBar.Margin = new Thickness(0, 0, 138, 0); // Don't cover up Window buttons
            }

            // Register ThemeHelper
            var themeHelper = ThemeHelper.RegisterWindowInstance(AppWindow);
            themeHelper!.UpdateTheme();

            // Set min size
            // TODO: Set min size
        }
    }
}
