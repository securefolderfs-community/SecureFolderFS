using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetPasswordPage : Page, IRecipient<PasswordClearRequestedMessage>
    {
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
            }

            base.OnNavigatedTo(e);
        }

        public void Receive(PasswordClearRequestedMessage message)
        {
            FirstPassword.Password = string.Empty;
            SecondPassword.Password = string.Empty;
        }
    }
}
