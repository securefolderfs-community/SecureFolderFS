using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class VaultWizardDialogViewModel : BaseDialogViewModel, IRecipient<VaultWizardNavigationRequestedMessage>
    {
        public IMessenger Messenger { get; }

        public BaseVaultWizardPageViewModel? CurrentPageViewModel { get; set; }

        public VaultViewModel? VaultViewModel { get; set; }

        public VaultWizardDialogViewModel()
        {
            Messenger = new WeakReferenceMessenger();
            Messenger.Register(this);

            PrimaryButtonClickCommand = new AsyncRelayCommand<IEventDispatchFlag?>(PrimaryButtonClick);
            SecondaryButtonClickCommand = new AsyncRelayCommand<IEventDispatchFlag?>(SecondaryButtonClick);
        }

        private Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.PrimaryButtonClick(flag) ?? Task.CompletedTask;
        }

        private Task SecondaryButtonClick(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.SecondaryButtonClick(flag) ?? Task.CompletedTask;
        }

        void IRecipient<VaultWizardNavigationRequestedMessage>.Receive(VaultWizardNavigationRequestedMessage message)
        {
            CurrentPageViewModel = message.Value;
        }
    }
}
