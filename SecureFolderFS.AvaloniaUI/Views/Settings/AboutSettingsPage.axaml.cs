using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class AboutSettingsPage : Page
    {
        public AboutSettingsPageViewModel? ViewModel
        {
            get => (AboutSettingsPageViewModel?)DataContext;
            set => DataContext = value;
        }

        public AboutSettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutSettingsPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}