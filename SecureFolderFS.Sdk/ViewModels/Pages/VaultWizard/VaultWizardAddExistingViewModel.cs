using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardAddExistingViewModel : VaultWizardPathSelectionBaseViewModel<ILocatableFolder>
    {
        public VaultWizardAddExistingViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
        }

        /// <inheritdoc/>
        public override Task PrimaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardSummaryViewModel(SelectedLocation!, Messenger, DialogViewModel)));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetLocation(ILocatableFolder storage)
        {
            var file = await storage.GetFileAsync(SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME);
            if (file is null)
                return false;

            await using var stream = await file.TryOpenStreamAsync(FileAccess.Read, FileShare.Read);
            var vaultConfig = RawVaultConfiguration.Load(stream);
            var isSupported = VaultVersion.IsVersionSupported(vaultConfig);
            if (isSupported)
            {
                LocationPath = storage.Path;
                SelectedLocation = storage;
                DialogViewModel.PrimaryButtonEnabled = true;
                return true;
            }

            return false;
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
