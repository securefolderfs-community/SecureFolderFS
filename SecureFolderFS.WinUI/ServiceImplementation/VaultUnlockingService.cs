using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultLoader.Routine;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class VaultUnlockingService : IVaultUnlockingService
    {
        private IFolder? _vaultFolder;
        private IVaultLoadRoutineStep3? _step3;
        private IVaultLoadRoutineStep8? _step8;

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        public Task<bool> SetVaultFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            _vaultFolder = folder;

            _step3 = VaultRoutines.NewVaultLoadRoutine()
                .EstablishRoutine()
                .AddVaultPath(new(folder.Path))
                .AddFileOperations();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetKeystoreStreamAsync(Stream stream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            _ = _step3 ?? throw new InvalidOperationException("The vault folder has not been set yet.");

            _step8 = _step3.FindConfigurationFile()
                .ContinueConfigurationFileInitialization()
                .FindKeystoreFile(true, new StreamKeystoreDiscoverer(stream))
                .ContinueKeystoreFileInitialization()
                .AddEncryptionAlgorithmBuilder();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IUnlockedVaultModel?> UnlockAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _ = _step8 ?? throw new InvalidOperationException("The keystore was has not been set yet.");

            try
            {
                var vaultInstance = _step8.DeriveMasterKeyFromPassword(new DisposablePassword(password.GetPassword()))
                    .ContinueInitializationWithMasterKey()
                    .VerifyVaultConfiguration()
                    .ContinueInitialization()
                    .Finalize()
                    .Deploy();

                var rootFolder = await FileSystemService.GetFolderFromPathAsync(vaultInstance.SecureFolderFSInstance.MountLocation);
                return new VaultInstanceUnlockedVaultModel(vaultInstance, _vaultFolder!);
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
