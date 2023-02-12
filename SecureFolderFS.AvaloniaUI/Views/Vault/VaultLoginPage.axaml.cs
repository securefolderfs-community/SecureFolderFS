using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;
using SecureFolderFS.UI.AppModels;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultLoginPage : Page
    {
        private Button? _continueButton;
        private TextBox? _vaultPasswordBox;

        public VaultLoginPageViewModel ViewModel
        {
            get => (VaultLoginPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultLoginPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultLoginPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async void VaultPasswordBox_OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await TryUnlock();
        }

        private void VaultPasswordBox_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _vaultPasswordBox = sender as TextBox;
        }

        private async void ContinueButton_OnClick(object? sender, RoutedEventArgs e)
        {
            await TryUnlock();
        }

        private void ContinueButton_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _continueButton = sender as Button;
        }

        private async Task TryUnlock()
        {
            if (_continueButton?.IsEnabled ?? false)
            {
                if (_vaultPasswordBox is null || _continueButton?.DataContext is not LoginCredentialsViewModel viewModel)
                    return;

                _continueButton.IsEnabled = false;
                await Task.Delay(25); // Wait for UI to update.

                var securePassword = string.IsNullOrEmpty(_vaultPasswordBox.Text) ? null : new SecurePassword(Encoding.UTF8.GetBytes(_vaultPasswordBox.Text));
                await Task.Run(() => viewModel.UnlockVaultCommand.ExecuteAsync(securePassword));

                await Task.Delay(25);
                _continueButton.IsEnabled = true;
            }
        }
    }
}