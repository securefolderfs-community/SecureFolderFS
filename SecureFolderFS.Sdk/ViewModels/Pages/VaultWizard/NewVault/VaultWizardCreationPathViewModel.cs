using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault
{
    public sealed class VaultWizardCreationPathViewModel : VaultWizardPathSelectionBaseViewModel<ILocatableFolder>
    {
        private VaultWizardPasswordViewModel? _nextViewModel;

        public VaultWizardCreationPathViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
        }

        public override Task PrimaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            // Cancel the confirm button
            flag?.NoForwarding();

            // We've already initialized the data
            if (_nextViewModel is not null)
            {
                Messenger.Send(new NavigationRequestedMessage(_nextViewModel));
                return Task.CompletedTask;
            }

            // Continue with initialization
            var step7 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultFolder(SelectedLocation)
                .AddFileOperations()
                .CreateConfigurationFile()
                .CreateKeystoreFile()
                .CreateContentFolder()
                .AddEncryptionAlgorithmBuilder();

            _nextViewModel = new(SelectedLocation, step7, Messenger, DialogViewModel);
            Messenger.Send(new NavigationRequestedMessage(_nextViewModel));

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<bool> SetLocation(ILocatableFolder storage)
        {
            SelectedLocation = storage;
            DialogViewModel.PrimaryButtonEnabled = true;

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        protected override async Task BrowseLocationAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickSingleFolderAsync();
            if (folder is not null)
                await SetLocation(folder);
        }
    }
}
