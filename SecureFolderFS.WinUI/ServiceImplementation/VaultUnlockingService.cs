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
using SecureFolderFS.Shared.Helpers;
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
        public Task<bool> SetConfigurationStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            _ = _step3 ?? throw new InvalidOperationException("The vault folder has not been set yet.");

            if (_folder is null)
                return Task.FromResult(false);

            _step5 = _step3
                .FindConfigurationFile(true, new StreamConfigDiscoverer(stream))
                .ContinueConfigurationFileInitialization();

            return Task.FromResult(true);
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
        public async Task<IResult<IUnlockedVaultModel?>> UnlockAndStartAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _ = _step8 ?? throw new InvalidOperationException("The keystore has not been set yet.");

            try
            {
                var vaultStatisticsBridge = new FileSystemStatsTrackerToVaultStatisticsModelBridge();

                var vaultInstance = _step8.DeriveMasterKeyFromPassword(password)
                    .ContinueInitializationWithMasterKey()
                    .VerifyVaultConfiguration()
                    .ContinueInitialization()
                    .Finalize()
                    .ContinueWithOptionalRoutine()
                    .EstablishOptionalRoutine()
                    .AddFileSystemStatsTracker(vaultStatisticsBridge)
                    .Finalize()
                    .Deploy();

                if (vaultInstance is null)
                    return new CommonResult<IUnlockedVaultModel?>(null);

                vaultInstance.SecureFolderFSInstance.StartFileSystem();
                await Task.Delay(100, cancellationToken); // Wait for the file system to start

                var rootFolder = await FileSystemService.GetFolderFromPathAsync(vaultInstance.SecureFolderFSInstance.MountLocation, cancellationToken);
                // TODO: Ensure rootFolder is not null
                return new CommonResult<IUnlockedVaultModel?>(new VaultInstanceUnlockedVaultModel(vaultInstance, rootFolder, vaultStatisticsBridge));
            }
            catch (Exception ex)
            {
                return new CommonResult<IUnlockedVaultModel?>(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
