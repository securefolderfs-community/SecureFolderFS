using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views
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
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as PageNavigationParameterModel)?.ViewModel is VaultLoginPageViewModel vaultLoginPageViewModel)
            {
                ViewModel = vaultLoginPageViewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void VaultPasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.UnlockVaultCommand.Execute((sender as PasswordBox)!.Password);
            }
        }
    }
}
