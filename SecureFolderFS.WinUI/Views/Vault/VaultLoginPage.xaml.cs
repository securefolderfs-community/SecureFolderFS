using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy;
using SecureFolderFS.WinUI.UserControls;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultLoginPage : Page
    {
        private Button? _continueButton;
        private PasswordControl? _passwordControl;

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

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            await TryUnlock();
        }

        private async Task TryUnlock()
        {
            if (_continueButton?.IsEnabled ?? false)
            {
                if (_passwordControl is null || _continueButton?.DataContext is not LoginCredentialsViewModel viewModel)
                    return;

                _continueButton.IsEnabled = false;
                await Task.Delay(25); // Wait for UI to update.

                var securePassword = _passwordControl.GetPassword();
                await viewModel.UnlockVaultCommand.ExecuteAsync(securePassword);

                await Task.Delay(25);
                _continueButton.IsEnabled = true;
            }
        }

        private void ContinueButton_Loaded(object sender, RoutedEventArgs e)
        {
            _continueButton = sender as Button;
        }

        private void PasswordControl_Loaded(object sender, RoutedEventArgs e)
        {
            _passwordControl = sender as PasswordControl;
        }
    }
}
