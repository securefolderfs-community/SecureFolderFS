using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.Authenticators;
using SecureFolderFS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public abstract class BaseVaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IEnumerable<IDisposable> passkey, VaultOptions vaultOptions,
            CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = VaultHelpers.ParseSecretKey(passkey);
            var options = VaultHelpers.ParseOptions(vaultOptions);

            var superSecret = await creationRoutine
                .SetCredentials(passkeySecret)
                .SetOptions(options)
                .FinalizeAsync(cancellationToken);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.TryCreateFileAsync(Constants.Vault.VAULT_README_FILENAME, false, cancellationToken);
                if (readmeFile is not null)
                    await readmeFile.WriteAllTextAsync(Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return superSecret;
        }

        /// <inheritdoc/>
        public async Task<IVaultLifecycle> UnlockAsync(IVaultModel vaultModel, IEnumerable<IDisposable> passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = VaultHelpers.ParseSecretKey(passkey);

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
            catch (Exception)
            {
                // Make sure to dispose the unlock contract when failed
                unlockContract.Dispose();

                throw;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<AuthenticationModel> GetAuthenticationAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            //yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, "Password", AuthenticationType.Password, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, "Key File", AuthenticationType.Other, new KeyFileAuthenticator());
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationModel> GetAvailableAuthenticationsAsync(CancellationToken cancellationToken = default);
    }
}
