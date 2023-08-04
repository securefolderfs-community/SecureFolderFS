using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Enums;
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
using System.Security.Principal;
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
            var availableAdapter = await GetBestAvailableAdapterAsync(cancellationToken);

            var storageService = Ioc.Default.GetRequiredService<IStorageService>();
            var statisticsBridge = new FileSystemStatisticsToVaultStatisticsModelBridge();

            var mountable = await storageRoutine
                .SetUnlockContract(unlockContract)
                .SetStorageService(storageService)
                .CreateMountableAsync(new FileSystemOptions()
                {
                    AdapterType = availableAdapter,
                    FileSystemStatistics = statisticsBridge,
                    VolumeName = vaultModel.VaultName // TODO: Format name to exclude illegal characters
                }, cancellationToken);

            var vaultInfoModel = new VaultInfoModel()
            {
                ContentCipherId = "TODO",
                FileNameCipherId = "TODO"
            };

            var virtualFileSystem = await mountable.MountAsync(GetMountOptions(availableAdapter), cancellationToken);
            return new VaultLifetimeModel(virtualFileSystem, statisticsBridge, vaultInfoModel);
        }

        private async Task<FileSystemAdapterType> GetBestAvailableAdapterAsync(CancellationToken cancellationToken)
        {
            var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            foreach (var item in vaultService.GetFileSystems())
            {
                if (item.Id == settingsService.UserSettings.PreferredFileSystemId && (await item.GetStatusAsync(cancellationToken)).Successful)
                    return GetAdapterType(item.Id);

                if ((await item.GetStatusAsync(cancellationToken)).Successful)
                    return GetAdapterType(item.Id);
            }

            throw new NotSupportedException("No supported adapters found");

            FileSystemAdapterType GetAdapterType(string fileSystemId)
            {
                return fileSystemId switch
                {
                    Core.Constants.FileSystemId.DOKAN_ID => FileSystemAdapterType.DokanAdapter,
                    Core.Constants.FileSystemId.FUSE_ID => FileSystemAdapterType.FuseAdapter,
                    Core.Constants.FileSystemId.WEBDAV_ID => FileSystemAdapterType.WebDavAdapter,
                };
            }
        }

        private MountOptions GetMountOptions(FileSystemAdapterType adapterType)
        {
            return adapterType switch
            {
                FileSystemAdapterType.DokanAdapter => new DokanyMountOptions(),
                FileSystemAdapterType.FuseAdapter => new FuseMountOptions(),
                FileSystemAdapterType.WebDavAdapter => new WebDavMountOptions() { Domain = "localhost", PreferredPort = 4949 },
                _ => throw new ArgumentOutOfRangeException(nameof(adapterType))
            };
        }
    }

    file sealed class CredentialsBuilder : ICredentialsBuilder
    {
        private IPassword? _password;
        private IIdentity? _identity;

        /// <inheritdoc/>
        public void Add(IDisposable authentication)
        {
            _password = authentication as IPassword ?? _password;
            _identity = authentication as IIdentity ?? _identity;
        }

        /// <inheritdoc/>
        public IDisposable BuildCredentials()
        {
            return new CredentialsCombo()
            {
                Password = _password,
                Authentication = _identity is IWrapper<SecretKey> authenticationData ? authenticationData.Inner : null
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _password?.Dispose();
            (_identity as IDisposable)?.Dispose();
        }
    }
}
