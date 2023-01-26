using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.UserControls;
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
    }
}