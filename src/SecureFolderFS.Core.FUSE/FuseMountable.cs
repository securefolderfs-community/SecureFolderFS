using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.Callbacks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;
using MountOptions = SecureFolderFS.Core.FileSystem.AppModels.MountOptions;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class FuseMountable : IMountableFileSystem, IAvailabilityChecker
    {
        private static readonly string MountDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SecureFolderFS), "mount");

        private readonly FuseWrapper _fuseWrapper;
        private readonly string _vaultName;

        private FuseMountable(OnDeviceFuse fuseCallbacks, string vaultName)
        {
            _fuseWrapper = new(fuseCallbacks);
            _vaultName = vaultName;
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            try
            {
                return Fuse.CheckDependencies() ? FileSystemAvailabilityType.Available : FileSystemAvailabilityType.ModuleNotAvailable;
            }
            catch (TypeInitializationException) // Fuse might sometimes throw (tested on Windows)
            {
                return FileSystemAvailabilityType.ModuleNotAvailable;
            }
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(MountDirectory))
                Directory.CreateDirectory(MountDirectory);

            if (mountOptions is not FuseMountOptions fuseMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(FuseMountOptions)}.");

            if (fuseMountOptions.AllowOtherUserAccess && fuseMountOptions.AllowRootUserAccess)
                throw new ArgumentException($"{nameof(FuseMountOptions)}.{nameof(fuseMountOptions.AllowOtherUserAccess)} and " +
                                            $"{nameof(FuseMountOptions)}.{nameof(fuseMountOptions.AllowRootUserAccess)} are mutually exclusive.");

            if ((fuseMountOptions.AllowOtherUserAccess || fuseMountOptions.AllowRootUserAccess) && !CanAllowOtherUsers())
                throw new ArgumentException($"{nameof(fuseMountOptions.AllowOtherUserAccess)} has been specified but user_allow_other is not uncommented " +
                                            "in /etc/fuse.conf.");

            var mountPoint = fuseMountOptions.MountPoint;
            if (mountPoint == null)
                Cleanup();
            else
                Cleanup(mountPoint);

            if (mountPoint != null && IsMountPoint(mountPoint))
                throw new ArgumentException("A filesystem is already mounted in the specified path.");

            if (mountPoint == null)
                mountPoint = GetFreeMountPoint(_vaultName);

            if (!Directory.Exists(mountPoint))
                Directory.CreateDirectory(mountPoint);

            _fuseWrapper.StartFileSystem(mountPoint, fuseMountOptions);
            var fuseFileSystem = new FuseFileSystem(_fuseWrapper, new SimpleFolder(mountPoint));

            return Task.FromResult<IVirtualFileSystem>(fuseFileSystem);
        }

        public static IMountableFileSystem CreateMountable(string vaultName, IFolder contentFolder, Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException("The vault content folder is not locatable.");

            var fuseCallbacks = new OnDeviceFuse(pathConverter, new(streamsAccess))
            {
                LocatableContentFolder = locatableContentFolder,
                Security = security,
                DirectoryIdAccess = directoryIdCache
            };

            return new FuseMountable(fuseCallbacks, vaultName);
        }

        /// <returns>Whether a filesystem is mounted in the specified directory.</returns>
        private static unsafe bool IsMountPoint(string directory)
        {
            stat stat = new();
            fixed (byte* pathPtr = Encoding.UTF8.GetBytes(directory))
            {
                if (LibC.stat(pathPtr, &stat) == -1)
                    return false;
            }

            stat parentStat = new();
            fixed (byte* parentPathPtr = Encoding.UTF8.GetBytes(Directory.GetParent(directory)!.FullName))
            {
                if (LibC.stat(parentPathPtr, &parentStat) == -1)
                    return false;
            }

            return stat.st_dev != parentStat.st_dev;
        }

        private static string GetFreeMountPoint(string vaultName)
        {
            var mountPoint = Path.Combine(MountDirectory, vaultName);

            var i = 1;
            while (IsMountPoint(mountPoint))
            {
                mountPoint = Path.Combine(MountDirectory, $"{vaultName} ({i++})");
            }

            return mountPoint;
        }

        /// <summary>
        /// Removes mount points without a connected transport endpoint in the default mount directory.
        /// </summary>
        private static void Cleanup()
        {
            foreach (var directory in Directory.GetDirectories(MountDirectory))
            {
                Cleanup(directory);
            }
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

        /// <returns>Whether the user_allow_other option is uncommented in /etc/fuse.conf</returns>
        private static bool CanAllowOtherUsers()
        {
            try
            {
                return File.ReadAllLines("/etc/fuse.conf").Any(x => x.Trim() == "user_allow_other");
            }
            catch
            {
                return false;
            }
        }
    }
}