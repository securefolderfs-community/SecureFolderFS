using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public abstract class BaseVaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions,
            CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);
            var options = VaultHelpers.ParseOptions(vaultOptions);

            var superSecret = await creationRoutine
                .SetCredentials(passkeySecret)
                .SetOptions(options)
                .FinalizeAsync(cancellationToken);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_README_FILENAME, false, cancellationToken);
                if (readmeFile is not null)
                    await readmeFile.WriteAllTextAsync(Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return superSecret;
        }

        /// <inheritdoc/>
        public async Task<IVaultLifecycle> UnlockAsync(IVaultModel vaultModel, IKey passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);

            await unlockRoutine.InitAsync(cancellationToken);

            var unlockContract = await unlockRoutine
                .SetCredentials(passkeySecret)
                .FinalizeAsync(cancellationToken);

            try
            {
                var storageRoutine = routines.BuildStorage();
                var fileSystemId = await VaultHelpers.GetBestFileSystemAsync(cancellationToken);

                var storageService = Ioc.Default.GetRequiredService<IStorageService>();
                var statisticsBridge = new FileSystemStatisticsToVaultStatisticsModelBridge();

                var mountable = await storageRoutine
                    .SetUnlockContract(unlockContract)
                    .SetStorageService(storageService)
                    .CreateMountableAsync(new FileSystemOptions()
                    {
                        FileSystemId = fileSystemId,
                        FileSystemStatistics = statisticsBridge,
                        VolumeName = vaultModel.VaultName // TODO: Format name to exclude illegal characters
                    }, cancellationToken);

                var vaultOptions = new VaultOptions()
                {
                    ContentCipherId = "TODO",
                    FileNameCipherId = "TODO",
                    AuthenticationMethod = "TODO",
                };

                var virtualFileSystem = await mountable.MountAsync(VaultHelpers.GetMountOptions(fileSystemId), cancellationToken);
                return new VaultLifetimeModel(virtualFileSystem, statisticsBridge, vaultOptions);
            }
            catch (Exception ex)
            {
                // Make sure to dispose the unlock contract when failed
                _ = ex;
                unlockContract.Dispose();

                throw;
            }
        }

        // TODO: Create a separate method that will determine the authentication method and return the correct auth method in the impl
        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetLoginAuthenticationAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetCreationAuthenticationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
