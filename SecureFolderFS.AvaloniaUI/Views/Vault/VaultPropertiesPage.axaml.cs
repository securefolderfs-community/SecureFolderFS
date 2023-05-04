using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultPropertiesPage : Page
    {
        public VaultPropertiesPageViewModel ViewModel
        {
            get => (VaultPropertiesPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultPropertiesPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultPropertiesPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}