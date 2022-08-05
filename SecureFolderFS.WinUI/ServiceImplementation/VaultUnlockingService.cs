using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultLoader.Routine;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class VaultUnlockingService : IVaultUnlockingService
    {
        private IVaultLoadRoutineStep3? _step3;
        private IVaultLoadRoutineStep5? _step5;
        private IVaultLoadRoutineStep8? _step8;
        private IFolder? _folder;

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        public Task<bool> SetVaultFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            _folder = folder;

            _step3 = VaultRoutines.NewVaultLoadRoutine()
                .EstablishRoutine()
                .SetFolder(folder)
                .AddFileOperations();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> SetConfigurationAsync(IFile configurationFile, CancellationToken cancellationToken = default)
        {
            _ = _step3 ?? throw new InvalidOperationException("The vault folder has not been set yet.");

            if (_folder is null)
                return false;

            var file = await _folder.GetFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            if (file is null)
                return false;

            await using var configStream = await file.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (configStream is null)
                return false;

            _step5 = _step3
                .FindConfigurationFile(true, new StreamConfigDiscoverer(configStream))
                .ContinueConfigurationFileInitialization();

            return true;
        }

        /// <inheritdoc/>
        public Task<bool> SetKeystoreStreamAsync(Stream stream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            _ = _step5 ?? throw new InvalidOperationException("The vault folder has not been set yet.");

            _step8 = _step5
                .FindKeystoreFile(true, new StreamKeystoreDiscoverer(stream))
                .ContinueKeystoreFileInitialization()
                .AddEncryptionAlgorithmBuilder();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IUnlockedVaultModel?> UnlockAndStartAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _ = _step8 ?? throw new InvalidOperationException("The keystore has not been set yet.");

            try
            {
                var vaultInstance = _step8.DeriveMasterKeyFromPassword(password)
                    .ContinueInitializationWithMasterKey()
                    .VerifyVaultConfiguration()
                    .ContinueInitialization()
                    .Finalize()
                    .Deploy();

                if (vaultInstance is null)
                    return null;

                vaultInstance.SecureFolderFSInstance.StartFileSystem();
                await Task.Delay(100, cancellationToken); // Wait for the file system to start

                var rootFolder = await FileSystemService.GetFolderFromPathAsync(vaultInstance.SecureFolderFSInstance.MountLocation);
                return new VaultInstanceUnlockedVaultModel(vaultInstance, rootFolder!);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
