using SecureFolderFS.Storage.VirtualFileSystem;
using System.Text;
using Tmds.Fuse;
using Tmds.Linux;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class FuseFileSystem
    {
        private static string MountDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SecureFolderFS), "mount");

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
