using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;
using System;
using System.Linq;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetPasswordPage : Page, IRecipient<PasswordClearRequestedMessage>, IDisposable
    {
        private bool _continueIgnore;

        public SetPasswordPageViewModel ViewModel
        {
            get => (SetPasswordPageViewModel)DataContext;
            set => DataContext = value;
        }

        public SetPasswordPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SetPasswordPageViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.Messenger.Register<PasswordClearRequestedMessage>(this);
                ViewModel.InitializeWithPassword = () => new(Encoding.UTF8.GetBytes(FirstPassword.Password));
            }

            // Always false since passwords are not preserved
            ViewModel.DialogViewModel.PrimaryButtonEnabled = false;
            ClearPasswords();

            base.OnNavigatedTo(e);
        }
        
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.Messenger.Unregister<PasswordClearRequestedMessage>(this);

            _continueIgnore = true;
            ClearPasswords();

            base.OnNavigatingFrom(e);
        }

        public void Receive(PasswordClearRequestedMessage message)
        {
            ClearPasswords();
        }

        private void ClearPasswords()
        {
            FirstPassword.Password = string.Empty;
            SecondPassword.Password = string.Empty;
        }

        private bool CanContinue()
        {
            return !string.IsNullOrEmpty(FirstPassword.Password) && FirstPassword.Password.SequenceEqual(SecondPassword.Password);
        }

        private void FirstPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_continueIgnore)
            {
                return;
            }

            ViewModel.DialogViewModel.PrimaryButtonEnabled = CanContinue();
        }

        private void SecondPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_continueIgnore)
            {
                return;
            }

            ViewModel.DialogViewModel.PrimaryButtonEnabled = CanContinue();
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }
    }
}
