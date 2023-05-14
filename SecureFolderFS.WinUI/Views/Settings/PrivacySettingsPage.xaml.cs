using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrivacySettingsPage : Page
    {
        private bool _isFirstTime = true;

        private ITelemetryService TelemetryService { get; } = Ioc.Default.GetRequiredService<ITelemetryService>();

        public PrivacySettingsViewModel ViewModel
        {
            get => (PrivacySettingsViewModel)DataContext;
            set => DataContext = value;
        }

        public PrivacySettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PrivacySettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async void EnableTelemetry_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                return;
            }

            if (EnableTelemetry.IsOn)
            {
                await TelemetryService.EnableTelemetryAsync();
                TelemetryService.TrackEvent("Telemetry manually enabled");
            }
            else
            {
                TelemetryService.TrackEvent("Telemetry manually disabled");
                await TelemetryService.DisableTelemetryAsync();
            }
        }
    }
}
