using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Views.VaultWizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IRecipient<VaultWizardNavigationRequestedMessage>
    {
        public VaultWizardDialogViewModel ViewModel
        {
            get => (VaultWizardDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultWizardDialog()
        {
            this.InitializeComponent();
        }

        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        public void Receive(VaultWizardNavigationRequestedMessage message)
        {
            NavigationTransitionInfo transition;
            switch (message.Value)
            {
                case VaultWizardMainPageViewModel:
                    transition = new SuppressNavigationTransitionInfo();
                    ContentFrame.Navigate(typeof(VaultWizardMainPage), message.Value, transition);
                    break;

                case AddExistingVaultPageViewModel:
                    transition = new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
                    ContentFrame.Navigate(typeof(AddExistingVaultPage), message.Value, transition); // TODO: Robust system for navigation? Based on indexes probably?
                    break;
            }
        }

        private void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Messenger.Register<VaultWizardNavigationRequestedMessage>(this);
            ViewModel.StartNavigation();
        }
    }
}
