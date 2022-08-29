using Microsoft.UI;
using Microsoft.UI.Windowing;
using SecureFolderFS.WinUI.Helpers;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.WindowViews
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable restore

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            EnsureEarlyWindow();
        }

        private void EnsureEarlyWindow()
        {
            // Set persistence id
            PersistenceId = "SecureFolderFS_mainwindow";

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
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(HostControl.CustomTitleBar);
            }

            // Register ThemeHelper
            var themeHelper = ThemeHelper.RegisterWindowInstance(AppWindow);
            themeHelper.UpdateTheme();

            // Set min size
            base.MinHeight = 572;
            base.MinWidth = 662;
        }
    }
}
