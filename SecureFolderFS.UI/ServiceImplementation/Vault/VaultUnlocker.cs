using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Cryptography.SecureStore;
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
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.UI.AppModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    /// <inheritdoc cref="IVaultUnlocker"/>
    public sealed class VaultUnlocker : IVaultUnlocker
    {
        /// <inheritdoc/>
        public ICredentialsBuilder GetCredentialsBuilder()
        {
            return new CredentialsBuilder();
        }

        /// <inheritdoc/>
        public async Task<IVaultLifetimeModel> UnlockAsync(IVaultModel vaultModel, IDisposable credentials, CancellationToken cancellationToken = default)
        {
            if (credentials is not CredentialsCombo credentialsCombo || credentialsCombo.Password is null)
                throw new ArgumentException("Credentials were not in a correct format.", nameof(credentials));

            var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            await unlockRoutine.InitAsync(cancellationToken);

            var unlockContract = await unlockRoutine
                .SetCredentials(credentialsCombo.Password, credentialsCombo.Authentication)
                .FinalizeAsync(cancellationToken);

            using var storageRoutine = routines.BuildStorage();
            var fileSystemId = await GetBestAvailableFileSystemAsync(cancellationToken);

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

            var vaultInfoModel = new VaultInfoModel()
            {
                ContentCipherId = "TODO",
                FileNameCipherId = "TODO"
            };

            var virtualFileSystem = await mountable.MountAsync(GetMountOptions(fileSystemId), cancellationToken);
            return new VaultLifetimeModel(virtualFileSystem, statisticsBridge, vaultInfoModel);
        }

        private async Task<string> GetBestAvailableFileSystemAsync(CancellationToken cancellationToken)
        {
            var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            string lastBestId = null;
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

    file sealed class CredentialsBuilder : ICredentialsBuilder
    {
        private IPassword? _password;
        private SecretKey? _authentication;

        /// <inheritdoc/>
        public void Add(IDisposable authentication)
        {
            _password = authentication as IPassword ?? _password;
            _authentication = authentication as SecretKey ?? _authentication;
        }

        /// <inheritdoc/>
        public IDisposable BuildCredentials()
        {
            return new CredentialsCombo()
            {
                Password = _password,
                Authentication = _authentication
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _password?.Dispose();
            _authentication?.Dispose();
        }
    }
}
