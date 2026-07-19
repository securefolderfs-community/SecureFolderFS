using System.Text;
using FuseSharp;
using FuseSharp.Native;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class MacFuseFileSystem
    {
        private static string MountDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SecureFolderFS), "mount");

        /// <returns>Whether a filesystem is mounted in the specified directory.</returns>
        private static unsafe bool IsMountPoint(string directory)
        {
            var parent = Directory.GetParent(directory)?.FullName;
            if (parent is null)
                return false;

            StatVfs directoryStat = default;
            fixed (byte* pathPtr = Encoding.UTF8.GetBytes(directory))
            {
                if (LibC.statvfs(pathPtr, &directoryStat) == -1)
                {
                    // A dead FUSE mount point cannot be stat-ed but is still a mount point
                    return LibC.errno is LibC.ENXIO or LibC.EIO;
                }
            }

            StatVfs parentStat = default;
            fixed (byte* parentPathPtr = Encoding.UTF8.GetBytes(parent))
            {
                if (LibC.statvfs(parentPathPtr, &parentStat) == -1)
                    return false;
            }

            return directoryStat.f_fsid != parentStat.f_fsid;
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

        /// <returns>Whether the allow_other macFUSE tunable is enabled.</returns>
        private static unsafe bool CanAllowOtherUsers()
        {
            var value = 0;
            var length = (nuint)sizeof(int);

            if (LibC.sysctlbyname("vfs.generic.macfuse.tunables.allow_other", &value, &length, null, 0) == -1)
                return false;

            return value != 0;
        }
    }
}
