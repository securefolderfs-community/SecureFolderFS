using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.MacFuse.AppModels;
using SecureFolderFS.Core.MacFuse.Callbacks;
using SecureFolderFS.Core.MacFuse.OpenHandles;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class MacFuseFileSystem : IFileSystemInfo
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }

        /// <inheritdoc/>
        public async Task<IVfsRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fuseOptions = MacFuseOptions.ToOptions(options.AppendContract(unlockContract));
            if (!Directory.Exists(MountDirectory))
                Directory.CreateDirectory(MountDirectory);

            if (fuseOptions is { AllowOtherUserAccess: true, AllowRootUserAccess: true })
                throw new ArgumentException($"{nameof(MacFuseOptions)}.{nameof(fuseOptions.AllowOtherUserAccess)} and {nameof(MacFuseOptions)}.{nameof(fuseOptions.AllowRootUserAccess)} are mutually exclusive.");

            if ((fuseOptions.AllowOtherUserAccess || fuseOptions.AllowRootUserAccess) && !CanAllowOtherUsers())
                throw new ArgumentException($"{nameof(fuseOptions.AllowOtherUserAccess)} has been specified but the allow_other macFUSE tunable is not enabled.");

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

            var handlesManager = new MacFuseHandlesManager(specifics.StreamsAccess, specifics.Options);
            var fuseCallbacks = new OnDeviceMacFuse(specifics, handlesManager);
            var fuseWrapper = new MacFuseWrapper(fuseCallbacks);
            fuseWrapper.StartFileSystem(mountPoint, fuseOptions);

            var virtualizedRoot = new SystemFolderEx(mountPoint);
            var plaintextRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new MacFuseVfsRoot(fuseWrapper, virtualizedRoot, plaintextRoot, specifics);
        }
    }
}
