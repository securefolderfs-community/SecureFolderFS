using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
using SecureFolderFS.UI.AppModels;
using System.Linq;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class PasswordWizardPage : Page
    {
        public VaultWizardPasswordViewModel? ViewModel
        {
            get => (VaultWizardPasswordViewModel?)DataContext;
            set => DataContext = value;
        }

        public PasswordWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardPasswordViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.InitializeWithPassword = () => new VaultPassword(FirstPassword.Text ?? string.Empty);
            }

            base.OnNavigatedTo(e);
        }

        private bool CanContinue()
        {
            return !string.IsNullOrEmpty(FirstPassword.Text) && FirstPassword.Text.SequenceEqual(SecondPassword.Text ?? string.Empty);
        }

        private void FirstPassword_PasswordChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.PrimaryButtonEnabled = CanContinue();
        }

        private void SecondPassword_PasswordChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.PrimaryButtonEnabled = CanContinue();
        }
    }
}