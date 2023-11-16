using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class PrivacySettingsPage : Page
    {
        public PrivacySettingsViewModel? ViewModel
        {
            get => (PrivacySettingsViewModel?)DataContext;
            set => DataContext = value;
        }

        public PrivacySettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PrivacySettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}