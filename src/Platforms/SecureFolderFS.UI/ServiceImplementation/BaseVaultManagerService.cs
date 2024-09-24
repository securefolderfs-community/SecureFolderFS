using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.Migration;
using SecureFolderFS.Core.Routines.Operational;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public abstract class BaseVaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions,
            CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);
            var options = VaultHelpers.ParseOptions(vaultOptions);

            await creationRoutine.InitAsync(cancellationToken);
            creationRoutine.SetCredentials(passkeySecret);
            creationRoutine.SetOptions(options);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.CreateFileAsync(Sdk.Constants.Vault.VAULT_README_FILENAME, true, cancellationToken);
                await readmeFile.WriteAllTextAsync(Sdk.Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return await creationRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKey passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);

            await unlockRoutine.InitAsync(cancellationToken);
            unlockRoutine.SetCredentials(passkeySecret);
            return await unlockRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> RecoverAsync(IFolder vaultFolder, string encodedMasterKey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            var recoveryRoutine = routines.RecoverVault();
            var keySplit = encodedMasterKey.Split(Core.Constants.KEY_TEXT_SEPARATOR);
            var masterKey = new SecureKey(Core.Cryptography.Constants.KeyChains.ENCKEY_LENGTH + Core.Cryptography.Constants.KeyChains.MACKEY_LENGTH);

            if (!Convert.TryFromBase64String(keySplit[0], masterKey.Key.AsSpan(0, Core.Cryptography.Constants.KeyChains.ENCKEY_LENGTH), out _))
                throw new FormatException("The master key (1) was not in correct format.");

            if (!Convert.TryFromBase64String(keySplit[1], masterKey.Key.AsSpan(Core.Cryptography.Constants.KeyChains.ENCKEY_LENGTH), out _))
                throw new FormatException("The master key (2) was not in correct format.");

            await recoveryRoutine.InitAsync(cancellationToken);
            recoveryRoutine.SetCredentials(masterKey);
            return await recoveryRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IVFSRoot> CreateLocalStorageAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            try
            {
                var contentFolder = await vaultModel.Folder.GetFolderByNameAsync(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
                var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
                var statisticsModel = new ConsolidatedStatisticsModel();
                var storageRoutine = routines.BuildStorage();
                var options = new FileSystemOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Format name to exclude illegal characters
                    FileSystemId = string.Empty,
                    HealthStatistics = statisticsModel,
                    FileSystemStatistics = statisticsModel
                };

                storageRoutine.SetUnlockContract(unlockContract);
                var specifics = storageRoutine.GetSpecifics(contentFolder, options);
                var cryptoFolder = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), contentFolder, specifics);

                return new LocalVFSRoot(specifics, cryptoFolder, options);
            }
            catch (Exception ex)
            {
                // Make sure to dispose the unlock contract when failed
                unlockContract.Dispose();

                _ = ex;
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IVaultMigratorModel> GetMigratorAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var configVersion = await vaultReader.ReadVersionAsync(cancellationToken);

            return configVersion.Version switch
            {
                Core.Constants.Vault.Versions.V1 => Migrators.GetMigratorV1_V2(vaultFolder, StreamSerializer.Instance),
                _ => throw new ArgumentOutOfRangeException(nameof(configVersion.Version))
            };
        }

        /// <inheritdoc/>
        public virtual async Task ChangeAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKey newPasskey,
            VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var credentialsRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).ModifyCredentials();
            using var newPasskeySecret = VaultHelpers.ParsePasskeySecret(newPasskey);
            var options = VaultHelpers.ParseOptions(vaultOptions);

            await credentialsRoutine.InitAsync(cancellationToken);
            credentialsRoutine.SetUnlockContract(unlockContract);
            credentialsRoutine.SetOptions(options);
            credentialsRoutine.SetCredentials(newPasskeySecret);
            
            _ = await credentialsRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public abstract Task<IVFSRoot> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken);
    }
}
