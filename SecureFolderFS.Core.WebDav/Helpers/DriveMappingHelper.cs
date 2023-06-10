using System.IO;
using System.Linq;
using System.Text;
using SecureFolderFS.Core.WebDav.UnsafeNative;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class DriveMappingHelper
    {
        /// <summary>
        /// Attempts to map a network drive. Doesn't throw on failure.
        /// </summary>
        public static Task MapNetworkDriveAsync(string mountPath, string remotePath, CancellationToken cancellationToken = default)
        {
            var netResource = new NETRESOURCE()
            {
                dwType = UnsafeNativeApis.RESOURCETYPE_DISK,
                lpLocalName = mountPath,
                lpRemoteName = remotePath,
            };

            // WNetAddConnection2 doesn't return until it has either successfully established a connection or timed out,
            // so it has to be run in another thread to prevent blocking the server from responding.
            return Task.Run(() => UnsafeNativeApis.WNetAddConnection2(netResource, null!, null!, UnsafeNativeApis.CONNECT_TEMPORARY), cancellationToken);
        }

        /// <summary>
        /// Attempts to disconnect a mapped network drive. Doesn't throw on failure.
        /// </summary>
        public static void DisconnectNetworkDrive(string mountPath, bool force)
        {
            _ = UnsafeNativeApis.WNetCancelConnection2(mountPath, 0, force);
        }

        public static string? GetMountPathForRemotePath(string remotePath)
        {
            remotePath = remotePath.TrimEnd('\\');

            var bufferSize = remotePath.Length + 1; // Null-terminated
            var driveRemotePathBuilder = new StringBuilder(bufferSize);

            foreach (var drive in DriveInfo.GetDrives().Select(item => $"{item.Name[0]}:"))
            {
                if (UnsafeNativeApis.WNetGetConnection(drive, driveRemotePathBuilder, ref bufferSize) == 0
                    && driveRemotePathBuilder.ToString() == remotePath)
                    return drive;

                driveRemotePathBuilder.Clear();
            }

            return null;
        }
    }
}
