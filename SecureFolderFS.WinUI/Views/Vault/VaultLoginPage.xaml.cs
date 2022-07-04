using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.WinUI.AppModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultLoginPage : Page
    {
        public VaultLoginPageViewModel ViewModel
        {
            get => (VaultLoginPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultLoginPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultLoginPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async void VaultPasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && ContinueButton.IsEnabled)
            {
                ContinueButton.IsEnabled = false;
                await Task.Delay(50); // Wait for UI to update.
                var securePassword = new SecurePassword(Encoding.UTF8.GetBytes(VaultPasswordBox.Password));
                ViewModel.UnlockVaultCommand.Execute(securePassword);
                ContinueButton.IsEnabled = true;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            var securePassword = new SecurePassword(Encoding.UTF8.GetBytes(VaultPasswordBox.Password));
            ViewModel.UnlockVaultCommand.Execute(securePassword);
            ContinueButton.IsEnabled = true;
        }
    }
}
