using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault
{
    public sealed class VaultWizardCreationPathViewModel : VaultWizardPathSelectionBaseViewModel<IModifiableFolder>
    {
        private readonly IVaultCreationModel _vaultCreationModel;
        private VaultWizardPasswordViewModel? _nextViewModel;

        public VaultWizardCreationPathViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _vaultCreationModel = new VaultCreationModel();
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken)
        {
            // Cancel the confirm button
            flag?.NoForwarding();

            // We've already initialized the data
            if (_nextViewModel is not null)
            {
                Messenger.Send(new NavigationRequestedMessage(_nextViewModel));
                return;
            }

            var keystoreFile = await SelectedLocation!.TryCreateFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, false, cancellationToken);
            if (keystoreFile is null)
                return; // TODO: Report issue

            // TODO: Important! Dispose the IKeystoreModel!!!
            var setKeystoreResult = await _vaultCreationModel.SetKeystoreAsync(new FileKeystoreModel(keystoreFile, StreamSerializer.Instance), cancellationToken);
            if (!setKeystoreResult.Successful)
                return; // TODO: Report issue

            _nextViewModel = new(_vaultCreationModel, Messenger, DialogViewModel);
            Messenger.Send(new NavigationRequestedMessage(_nextViewModel));
        }

        /// <inheritdoc/>
        public override async Task<bool> SetLocationAsync(IModifiableFolder storage, CancellationToken cancellationToken = default)
        {
            var setFolderResult = await _vaultCreationModel.SetFolderAsync(storage, cancellationToken);
            if (!setFolderResult.Successful)
                return false;

            SelectedLocation = storage;
            VaultName = storage.Name;
            DialogViewModel.PrimaryButtonEnabled = true;
            return true;
        }

        /// <inheritdoc/>
        protected override async Task BrowseLocationAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickSingleFolderAsync(cancellationToken);
            if (folder is IModifiableFolder modifiableFolder)
                await SetLocationAsync(modifiableFolder, cancellationToken);
        }
    }
}
