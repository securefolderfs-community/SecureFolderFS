using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed partial class VaultWizardDialogViewModel : DialogViewModel, IRecipient<NavigationRequestedMessage>
    {
        public IMessenger Messenger { get; }

        public BaseVaultWizardPageViewModel? CurrentPageViewModel { get; set; }

        public VaultWizardDialogViewModel()
        {
            Messenger = new WeakReferenceMessenger();
            Messenger.Register(this);
        }

        /// <inheritdoc/>
        public void Receive(NavigationRequestedMessage message)
        {
            CurrentPageViewModel = message.ViewModel as BaseVaultWizardPageViewModel;
        }

        [RelayCommand]
        private Task PrimaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.PrimaryButtonClickAsync(flag) ?? Task.CompletedTask;
        }

        [RelayCommand]
        private Task SecondaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.SecondaryButtonClickAsync(flag) ?? Task.CompletedTask;
        }

        [RelayCommand]
        private void GoBack()
        {
            Messenger.Send(new BackNavigationRequestedMessage());
        }
    }
}
