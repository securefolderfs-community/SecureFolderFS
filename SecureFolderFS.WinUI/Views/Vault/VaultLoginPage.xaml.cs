using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages;
using System.Text;
using Microsoft.UI.Xaml;
using SecureFolderFS.Core.PasswordRequest;

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
            {
                ViewModel = viewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void VaultPasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && ContinueButton.IsEnabled)
            {
                ContinueButton.IsEnabled = false;
                var disposablePassword = new DisposablePassword(Encoding.UTF8.GetBytes(VaultPasswordBox.Password));
                ViewModel.UnlockVaultCommand.Execute(disposablePassword);
                ContinueButton.IsEnabled = true;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            var disposablePassword = new DisposablePassword(Encoding.UTF8.GetBytes(VaultPasswordBox.Password));
            ViewModel.UnlockVaultCommand.Execute(disposablePassword);
            ContinueButton.IsEnabled = true;
        }
    }
}
