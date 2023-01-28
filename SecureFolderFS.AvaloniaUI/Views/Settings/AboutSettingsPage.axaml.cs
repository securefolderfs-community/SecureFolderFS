using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class AboutSettingsPage : Page
    {
        public AboutSettingsPageViewModel ViewModel
        {
            get => (AboutSettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public AboutSettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutSettingsPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void VersionButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyVersionCommand?.Execute(null);
            VersionCopiedTeachingTip.IsOpen = true;
        }

        private async void DiscordButton_OnClick(object? sender, RoutedEventArgs e)
        {
            // SettingsExpander sets e.Handled to true, which prevents the command from executing.
            await ViewModel.OpenDiscordSocialCommand.ExecuteAsync(null);
        }
    }
}