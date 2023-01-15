using System.Diagnostics;
using System.Text;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.Callbacks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using Tmds.Fuse;
using Tmds.Linux;
using MountOptions = SecureFolderFS.Core.FileSystem.AppModels.MountOptions;

namespace SecureFolderFS.Core.FUSE.Mounters
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class FuseMountable : IMountableFileSystem
    {
        private static readonly string DefaultMountDirectory = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SecureFolderFS), "mount");

        private readonly FuseWrapper _fuseWrapper;
        private readonly string _vaultName;

        private FuseMountable(OnDeviceFuse fuseCallbacks, string vaultName)
        {
            _fuseWrapper = new(fuseCallbacks);
            _vaultName = vaultName;
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not FuseMountOptions fuseMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(FuseMountOptions)}.");

            var mountPoint = fuseMountOptions.MountPoint;
            if (mountPoint == null)
                Cleanup();
            else
                Cleanup(mountPoint);

            if (mountPoint != null && IsMountPoint(mountPoint))
                throw new ArgumentException("A filesystem is already mounted in the specified path.");

            if (mountPoint == null)
            {
                mountPoint = Path.Combine(DefaultMountDirectory, _vaultName);

                var i = 1;
                while (IsMountPoint(mountPoint))
                    mountPoint = Path.Combine(DefaultMountDirectory, $"{_vaultName} ({i++})");
            }

            if (!Directory.Exists(mountPoint))
                Directory.CreateDirectory(mountPoint);

            _fuseWrapper.StartFileSystem(mountPoint);
            var fuseFileSystem = new FuseFileSystem(_fuseWrapper, new SimpleFolder(mountPoint));

            return Task.FromResult<IVirtualFileSystem>(fuseFileSystem);
        }

        public static IMountableFileSystem CreateMountable(string vaultName, IPathConverter pathConverter, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IStreamsAccess streamsAccess)
        {
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException("The vault content folder is not locatable.");

            var fuseCallbacks = new OnDeviceFuse(pathConverter, new(streamsAccess))
            {
                LocatableContentFolder = locatableContentFolder,
                Security = security,
                DirectoryIdAccess = directoryIdAccess
            };

            return new FuseMountable(fuseCallbacks, vaultName);
        }

        /// <returns>Whether a filesystem is mounted in the specified directory.</returns>
        private static unsafe bool IsMountPoint(string directory)
        {
            stat stat = new();
            fixed (byte *pathPtr = Encoding.UTF8.GetBytes(directory))
                if (LibC.stat(pathPtr, &stat) == -1)
                    return false;

            stat parentStat = new();
            fixed (byte *parentPathPtr = Encoding.UTF8.GetBytes(Directory.GetParent(directory)!.FullName))
                if (LibC.stat(parentPathPtr, &parentStat) == -1)
                    return false;

            return stat.st_dev != parentStat.st_dev;
        }

        /// <summary>
        /// Removes mount points without a connected transport endpoint in the default mount directory.
        /// </summary>
        private static void Cleanup()
        {
            foreach (var directory in Directory.GetDirectories(DefaultMountDirectory))
                Cleanup(directory);
        }

        /// <summary>
        /// Unmounts and deletes the specified directory if it is a mount point without a connected transport endpoint.
        /// Otherwise, nothing happens.
        /// </summary>
        private static void Cleanup(string directory)
        {
            try
            {
                Directory.EnumerateFileSystemEntries(directory);
            }
            catch (IOException)
            {
                Fuse.LazyUnmount(directory);
                Directory.Delete(directory);
            }
        }
    }
}