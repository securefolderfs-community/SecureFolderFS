using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.WinUI.Helpers;
using System.Threading.Tasks;
using SecureFolderFS.UI;
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
            PersistenceId = Constants.MAIN_WINDOW_ID;

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
            ThemeHelper.Instance.RegisterWindowInstance(AppWindow, HostControl);

            // Set min size
            base.MinHeight = 572;
            base.MinWidth = 662;

            // Hook up event for window closing
            AppWindow.Closing += AppWindow_Closing;
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var applicationSettingsService = Ioc.Default.GetRequiredService<IApplicationSettingsService>();

            await Task.WhenAll(settingsService.SaveSettingsAsync(), applicationSettingsService.SaveSettingsAsync());
        }

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Update current theme to refresh window buttons' colors
            ThemeHelper.Instance.UpdateTheme();
        }
    }
}
