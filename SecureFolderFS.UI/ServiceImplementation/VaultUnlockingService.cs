using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation
{
    public sealed class VaultUnlockingService : IVaultUnlockingService
    {
        private IUnlockRoutine? _unlockRoutine;
        private FileSystemAdapterType _fileSystemAdapterType;

        /// <inheritdoc/>
        public async Task<IResult> SetVaultFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            _unlockRoutine?.Dispose();
            _unlockRoutine ??= VaultHelpers.NewUnlockRoutine();

            try
            {
                var storageService = Ioc.Default.GetRequiredService<IStorageService>();
                await _unlockRoutine.SetVaultStoreAsync(vaultFolder, storageService, cancellationToken);
                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> SetConfigurationStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (_unlockRoutine is null)
                return new CommonResult(false);

            try
            {
                await _unlockRoutine.ReadConfigurationAsync(stream, StreamSerializer.Instance, cancellationToken);
                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreStreamAsync(Stream stream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            if (_unlockRoutine is null)
                return new CommonResult(false);

            try
            {
                await _unlockRoutine.ReadKeystoreAsync(stream, serializer, cancellationToken);
                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> SetFileSystemAsync(IFileSystemInfoModel fileSystemInfoModel, CancellationToken cancellationToken = default)
        {
            // Check if the file system is supported
            var isSupportedResult = await fileSystemInfoModel.IsSupportedAsync(cancellationToken);
            if (!isSupportedResult.Successful)
                return isSupportedResult;

            // Determine file system adapter type
            _fileSystemAdapterType = fileSystemInfoModel.Id switch
            {
                Core.Constants.FileSystemId.DOKAN_ID => FileSystemAdapterType.DokanAdapter,
                Core.Constants.FileSystemId.FUSE_ID => FileSystemAdapterType.FuseAdapter,
                Core.Constants.FileSystemId.WEBDAV_ID => FileSystemAdapterType.WebDavAdapter,
                _ => throw new ArgumentOutOfRangeException(nameof(IFileSystemInfoModel.Id))
            };

            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IResult<IUnlockedVaultModel?>> UnlockAndStartAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            if (_unlockRoutine is null)
                return new CommonResult<IUnlockedVaultModel?>(null, false);

            try
            {
                // Derive cryptographic keys from password
                _unlockRoutine.DeriveKeystore(password);

                // Retrieve file system mounter
                var vaultStatisticsBridge = new FileSystemStatisticsToVaultStatisticsModelBridge();
                var mountableFileSystem = await _unlockRoutine.PrepareAndUnlockAsync(new()
                {
                    AdapterType = _fileSystemAdapterType,
                    FileSystemStatistics = vaultStatisticsBridge
                }, cancellationToken);

                // Select appropriate mount options for the file system
                MountOptions mountOptions = _fileSystemAdapterType switch
                {
                    FileSystemAdapterType.DokanAdapter => new DokanyMountOptions(),
                    FileSystemAdapterType.WebDavAdapter => new WebDavMountOptions() { Domain = "localhost", Port = "4949" }
                };

                // Mount the file system
                var virtualFileSystem = await mountableFileSystem.MountAsync(mountOptions, cancellationToken);

                return new CommonResult<IUnlockedVaultModel?>(new FileSystemUnlockedVaultModel(virtualFileSystem, vaultStatisticsBridge));
            }
            catch (Exception ex)
            {
                return new CommonResult<IUnlockedVaultModel?>(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _unlockRoutine?.Dispose();
        }
    }
}
