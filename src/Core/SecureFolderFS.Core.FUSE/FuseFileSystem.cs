using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.Callbacks;
using SecureFolderFS.Core.FUSE.OpenHandles;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class FuseFileSystem : IFileSystemInfo
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
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fuseOptions = FuseOptions.ToOptions(options.AppendContract(unlockContract));
            if (!Directory.Exists(MountDirectory))
                Directory.CreateDirectory(MountDirectory);

            if (fuseOptions is { AllowOtherUserAccess: true, AllowRootUserAccess: true })
                throw new ArgumentException($"{nameof(FuseOptions)}.{nameof(fuseOptions.AllowOtherUserAccess)} and {nameof(FuseOptions)}.{nameof(fuseOptions.AllowRootUserAccess)} are mutually exclusive.");

            if ((fuseOptions.AllowOtherUserAccess || fuseOptions.AllowRootUserAccess) && !CanAllowOtherUsers())
                throw new ArgumentException($"{nameof(fuseOptions.AllowOtherUserAccess)} has been specified but user_allow_other is not uncommented in /etc/fuse.conf.");

            var mountPoint = fuseOptions.MountPoint;
            if (mountPoint == null)
                Cleanup();
            else
                Cleanup(mountPoint);

            if (mountPoint != null && IsMountPoint(mountPoint))
                throw new ArgumentException("A filesystem is already mounted in the specified path.");

            mountPoint ??= GetFreeMountPoint(fuseOptions.VolumeName);
            if (!Directory.Exists(mountPoint))
                Directory.CreateDirectory(mountPoint);

            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fuseOptions);
            fuseOptions.SetupValidators(specifics);

            var handlesManager = new FuseHandlesManager(specifics.StreamsAccess, specifics.Options);
            var fuseCallbacks = new OnDeviceFuse(specifics, handlesManager);
            var fuseWrapper = new FuseWrapper(fuseCallbacks);
            fuseWrapper.StartFileSystem(mountPoint, fuseOptions);

            await Task.CompletedTask;
            return new FuseVFSRoot(fuseWrapper, new SystemFolderEx(mountPoint), specifics);
        }
    }
}
