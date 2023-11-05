using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    /// <inheritdoc cref="IVaultUnlocker"/>
    public sealed class VaultUnlocker : IVaultUnlocker
    {
        /// <inheritdoc/>
        public async Task<IVaultLifecycle> UnlockAsync(IVaultModel vaultModel, IEnumerable<IDisposable> passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = AuthenticationHelpers.ParseSecretKey(passkey);

            await unlockRoutine.InitAsync(cancellationToken);

            var unlockContract = await unlockRoutine
                .SetCredentials(passkeySecret)
                .FinalizeAsync(cancellationToken);

            try
            {
                var storageRoutine = routines.BuildStorage();
                var fileSystemId = await GetBestFileSystemAsync(cancellationToken);

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
                    Specialization = "TODO"
                };

                var virtualFileSystem = await mountable.MountAsync(GetMountOptions(fileSystemId), cancellationToken);
                return new VaultLifetimeModel(virtualFileSystem, statisticsBridge, vaultOptions);
            }
            catch (Exception)
            {
                // Make sure to dispose the unlock contract when failed
                unlockContract.Dispose();

                throw;
            }
        }

        private async Task<string> GetBestFileSystemAsync(CancellationToken cancellationToken)
        {
            var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            string? lastBestId = null;
            foreach (var item in vaultService.GetFileSystems())
            {
                if (item.Id == settingsService.UserSettings.PreferredFileSystemId)
                {
                    if ((await item.GetStatusAsync(cancellationToken)).Successful)
                        return item.Id;
                }
                else
                {
                    if (lastBestId is null && (await item.GetStatusAsync(cancellationToken)).Successful)
                        lastBestId = item.Id;
                }
            }

            if (lastBestId is null)
                throw new NotSupportedException("No supported adapters found.");

            return lastBestId;
        }

        private MountOptions GetMountOptions(string fileSystemId)
        {
            return fileSystemId switch
            {
                Core.Constants.FileSystemId.DOKAN_ID => new DokanyMountOptions(),
                Core.Constants.FileSystemId.FUSE_ID => new FuseMountOptions(),
                Core.Constants.FileSystemId.WEBDAV_ID => new WebDavMountOptions() { Domain = "localhost", PreferredPort = 4949 },
                _ => throw new ArgumentOutOfRangeException(nameof(fileSystemId))
            };
        }
    }
}
