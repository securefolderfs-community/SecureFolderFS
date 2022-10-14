using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class VaultUnlockingService : IVaultUnlockingService
    {
        private IFolder? _vaultFolder;
        private IUnlockRoutine? _unlockRoutine;
        private FileSystemAdapterType _fileSystemAdapterType;

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        public async Task<IResult> SetVaultFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            _unlockRoutine?.Dispose();
            _unlockRoutine ??= VaultRoutines.NewUnlockRoutine();

            try
            {
                await _unlockRoutine.SetVaultFolder(vaultFolder, cancellationToken);
                _vaultFolder = vaultFolder;

                return new CommonResult();
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
                return new CommonResult();
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
                return new CommonResult();
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> SetFileSystemAsync(IFileSystemInfoModel fileSystemInfoModel, CancellationToken cancellationToken = default)
        {
            var isSupportedResult = await fileSystemInfoModel.IsSupportedAsync(cancellationToken);
            if (!isSupportedResult.Successful)
                return isSupportedResult;

            // Determine file system adapter type
            _fileSystemAdapterType = fileSystemInfoModel.Id switch
            {
                Core.Constants.FileSystemId.DOKAN_ID => FileSystemAdapterType.DokanAdapter,
                Core.Constants.FileSystemId.WEBDAV_ID => FileSystemAdapterType.WebDavAdapter,
                _ => throw new ArgumentOutOfRangeException(nameof(IFileSystemInfoModel.Id))
            };

            return new CommonResult();
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
                var vaultStatisticsBridge = new FileSystemStatsTrackerToVaultStatisticsModelBridge();
                var mountableFileSystem = await _unlockRoutine.PrepareAndUnlockAsync(new()
                {
                    FileSystemAdapterType = _fileSystemAdapterType,
                    FileSystemStatsTracker = vaultStatisticsBridge
                }, cancellationToken);

                // Get free mount point
                var firstFreeMountPoint = FileSystemService.GetFreeMountPoints().FirstOrDefault();
                if (firstFreeMountPoint is null)
                    return new CommonResult<IUnlockedVaultModel?>(new DirectoryNotFoundException("No available free mount points for vault file system"));
                
                // Mount the file system
                var virtualFileSystem = await mountableFileSystem.MountAsync(new(firstFreeMountPoint), cancellationToken);
                
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
        }
    }
}
