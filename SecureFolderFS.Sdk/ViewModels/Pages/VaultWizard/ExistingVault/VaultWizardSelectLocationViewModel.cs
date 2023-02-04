using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.ExistingVault
{
    public sealed class VaultWizardSelectLocationViewModel : VaultWizardPathSelectionBaseViewModel<ILocatableFolder>
    {
        private readonly IVaultExistingCreationModel _vaultExistingCreationModel;

        public VaultWizardSelectLocationViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _vaultExistingCreationModel = new VaultExistingCreationModel();
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken)
        {
            flag?.NoForwarding();

            var deployResult = await _vaultExistingCreationModel.DeployAsync(cancellationToken);
            if (!deployResult.Successful)
                return; // TODO: Report issue

            // TODO: Handle adding vault to VaultCollectionModel here...

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardSummaryViewModel(deployResult.Value!, Messenger, DialogViewModel)));
        }

        /// <inheritdoc/>
        public override async Task<bool> SetLocationAsync(ILocatableFolder storage, CancellationToken cancellationToken = default)
        {
            var setFolderResult = await _vaultExistingCreationModel.SetFolderAsync(storage, cancellationToken);
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
            if (folder is not null)
                await SetLocationAsync(folder, cancellationToken);
        }
    }
}
