using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.WinFsp.AppModels;
using SecureFolderFS.Core.WinFsp.Callbacks;
using SecureFolderFS.Core.WinFsp.OpenHandles;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class WinFspFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var winFspOptions = WinFspOptions.ToOptions(options.AppendContract(unlockContract));
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, winFspOptions);
            winFspOptions.SetupValidators(specifics);

            if (winFspOptions.MountPoint is null)
                winFspOptions.DangerousSetMountPoint(PathHelpers.GetFreeMountPath(winFspOptions.VolumeName));

            if (winFspOptions.MountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            var handlesManager = new WinFspHandlesManager(specifics.Security, specifics.StreamsAccess, specifics.Options);
            var volumeModel = new VolumeModel(specifics.Options.VolumeName, Constants.WinFsp.FS_TYPE_ID);
            var pathRoot = Path.GetPathRoot(specifics.ContentFolder.Id) ?? throw new InvalidOperationException("Content folder has an invalid path.");
            var driveInfo = new DriveInfo(pathRoot);
            var winFspCallbacks = new OnDeviceWinFsp(specifics, handlesManager, volumeModel, driveInfo);
            var winFspService = new WinFspService(winFspCallbacks, winFspOptions.MountPoint);
            var result = await winFspService.StartFileSystemAsync();

            if (!result.Successful)
                throw new Win32Exception(result.Value);

            winFspOptions.DangerousSetMountPoint(winFspService.GetMountPointInternal());
            return new WinFspVFSRoot(winFspService, new SystemFolderEx(winFspOptions.MountPoint), specifics);
        }
    }
}
