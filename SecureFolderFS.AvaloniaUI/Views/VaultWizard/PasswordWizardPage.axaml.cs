using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class PasswordWizardPage : Page
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardPasswordViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.InitializeWithPassword = () => new SecurePassword(Encoding.UTF8.GetBytes(FirstPassword.Text ?? string.Empty));
            }

            base.OnNavigatedTo(e);
        }

        private bool CanContinue()
        {
            return !string.IsNullOrEmpty(FirstPassword.Text) && FirstPassword.Text.SequenceEqual(SecondPassword.Text ?? string.Empty);
        }

        private void FirstPassword_PasswordChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = CanContinue();
        }

        private void SecondPassword_PasswordChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = CanContinue();
        }
    }
}