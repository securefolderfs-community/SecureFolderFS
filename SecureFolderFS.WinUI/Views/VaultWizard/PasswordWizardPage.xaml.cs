using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using System;
using System.Linq;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PasswordWizardPage : Page, IDisposable
    {
        public VaultWizardPasswordViewModel ViewModel
        {
            get => (VaultWizardPasswordViewModel)DataContext;
            set => DataContext = value;
        }

        public PasswordWizardPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardPasswordViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.InitializeWithPassword = () => new(Encoding.UTF8.GetBytes(FirstPassword.Password));
            }

            base.OnNavigatedTo(e);
        }

        private bool CanContinue()
        {
            return !string.IsNullOrEmpty(FirstPassword.Password) && FirstPassword.Password.SequenceEqual(SecondPassword.Password);
        }

        private void FirstPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = CanContinue();
        }

        private void SecondPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = CanContinue();
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }
    }
}
