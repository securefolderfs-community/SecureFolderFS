using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        private IVaultUnlockingService VaultUnlockingService { get; } = Ioc.Default.GetRequiredService<IVaultUnlockingService>();

        public string VaultName => VaultModel.VaultName;

        public IAsyncRelayCommand UnlockVaultCommand { get; }

        public VaultLoginPageViewModel(IVaultModel vaultModel)
            : base(vaultModel, new WeakReferenceMessenger())
        {
            UnlockVaultCommand = new AsyncRelayCommand<IPassword?>(UnlockVaultAsync);
        }

        private async Task UnlockVaultAsync(IPassword? password, CancellationToken cancellationToken = default)
        {
            if (password is null)
                return;

            if (!await VaultModel.IsAccessibleAsync())
                return; // TODO: Report the issue

            // Try set the lock
            _ = await VaultModel.LockFolderAsync();

            IUnlockedVaultModel? unlockedVaultModel;
            using (VaultModel.FolderLock)
            using (password)
            {
                // Set the vault folder
                if (!await VaultUnlockingService.SetVaultFolderAsync(VaultModel.Folder, cancellationToken))
                    return; // TODO: Report the issue

                // Get the keystore stream
                var keystoreStream = await GetKeystoreStreamAsync(VaultModel.Folder, cancellationToken);
                if (keystoreStream is null)
                    return;

                // Set the keystore stream
                if (!await VaultUnlockingService.SetKeystoreStreamAsync(keystoreStream, JsonToStreamSerializer.Instance, cancellationToken))
                    return; // TODO: Report the issue

                // Unlock the vault
                unlockedVaultModel = await VaultUnlockingService.UnlockAsync(password, cancellationToken);
                if (unlockedVaultModel is null)
                    return; // TODO: Report incorrect password
            }

            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(new VaultDashboardPageViewModel(unlockedVaultModel, VaultModel, Messenger)));
        }

        private static async Task<Stream?> GetKeystoreStreamAsync(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            _ = cancellationToken; // TODO: Cancellation token here will be used later

            var keystoreFile = await vaultFolder.GetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME);
            if (keystoreFile is not null)
                return await keystoreFile.OpenStreamAsync(FileAccess.Read, FileShare.Read);

            // TODO: Get the stream another way
            return null;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            VaultUnlockingService.Dispose();
        }
    }
}
