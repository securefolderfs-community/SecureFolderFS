using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using SecureFolderFS.WinUI.Helpers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window // TODO: Remove SecureFolderFS.Core reference from the UI
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            PrepareWindow();
        }

        private void PrepareWindow()
        {
            // Get AppWindow
            var hWnd = WindowNative.GetWindowHandle(this);
            var mainWindowWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(mainWindowWndId);

            // Set title
            appWindow.Title = "SecureFolderFS";

            // Extend title bar
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            // Set window buttons background to transparent
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Register ThemeHelper
            var themeHelper = ThemeHelper.RegisterWindowInstance(appWindow);
            themeHelper!.UpdateTheme();
        }
    }
}
