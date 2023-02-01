using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class PrivacySettingsPage : Page
    {
        public PrivacySettingsPageViewModel ViewModel
        {
            get => (PrivacySettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public PrivacySettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PrivacySettingsPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}