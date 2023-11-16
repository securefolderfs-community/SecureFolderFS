using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class PasswordWizardPage : Page
    {
        public PasswordWizardViewModel? ViewModel
        {
            get => (PasswordWizardViewModel?)DataContext;
            set => DataContext = value;
        }

        public PasswordWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PasswordWizardViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.InitializeWithPassword = FirstPassword.GetPassword;
            }

            base.OnNavigatedTo(e);
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword.PasswordInput.Text) && FirstPassword.Equals(SecondPassword);
        }
    }
}