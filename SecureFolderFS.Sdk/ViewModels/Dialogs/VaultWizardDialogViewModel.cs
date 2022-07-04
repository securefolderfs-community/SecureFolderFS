using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class VaultWizardDialogViewModel : DialogViewModel, IRecipient<NavigationRequestedMessage>
    {
        public IMessenger Messenger { get; }

        public BaseVaultWizardPageViewModel? CurrentPageViewModel { get; set; }

        public IRelayCommand GoBackCommand { get; }

        public VaultWizardDialogViewModel()
        {
            Messenger = new WeakReferenceMessenger();
            Messenger.Register(this);

            PrimaryButtonClickCommand = new AsyncRelayCommand<IEventDispatchFlag?>(PrimaryButtonClickAsync);
            SecondaryButtonClickCommand = new AsyncRelayCommand<IEventDispatchFlag?>(SecondaryButtonClickAsync);
            GoBackCommand = new RelayCommand(GoBack);
        }

        /// <inheritdoc/>
        public void Receive(NavigationRequestedMessage message)
        {
            CurrentPageViewModel = message.ViewModel as BaseVaultWizardPageViewModel;
        }

        private Task PrimaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.PrimaryButtonClickAsync(flag) ?? Task.CompletedTask;
        }

        private Task SecondaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            return CurrentPageViewModel?.SecondaryButtonClickAsync(flag) ?? Task.CompletedTask;
        }

        private void GoBack()
        {
            Messenger.Send(new BackNavigationRequestedMessage());
        }
    }
}
