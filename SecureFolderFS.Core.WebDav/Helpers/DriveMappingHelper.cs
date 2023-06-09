using SecureFolderFS.Core.WebDav.UnsafeNative;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class DriveMappingHelper
    {
        public static IEnumerable<char> GetAvailableDriveLetters()
        {
            return Enumerable.Range('A', 'Z' - 'A' + 1)
                .Select(x => (char)x)
                .Except(DriveInfo.GetDrives().Select(x => x.Name[0]))
                .OrderByDescending(x => x);
        }

        /// <summary>
        /// Attempts to map a network drive. Doesn't throw on failure.
        /// </summary>
        public static Task MapNetworkDriveAsync(char driveLetter, string remotePath, CancellationToken cancellationToken = default)
        {
            if (driveLetter < 'A' || driveLetter > 'Z')
                throw new ArgumentOutOfRangeException(nameof(driveLetter));

            if (!GetAvailableDriveLetters().Contains(driveLetter))
                throw new ArgumentException("The specified drive letter is already in use.", nameof(driveLetter));

            var netResource = new NETRESOURCE()
            {
                dwType = UnsafeNativeApis.RESOURCETYPE_DISK,
                lpLocalName = $"{driveLetter}:",
                lpRemoteName = remotePath,
            };

            // WNetAddConnection2 doesn't return until it has either successfully established a connection or timed out,
            // so it has to be run in another thread to prevent blocking the server from responding.
            return Task.Run(() => UnsafeNativeApis.WNetAddConnection2(netResource, null!, null!, UnsafeNativeApis.CONNECT_TEMPORARY), cancellationToken);
        }

        /// <summary>
        /// Attempts to disconnect a mapped network drive. Doesn't throw on failure.
        /// </summary>
        public static void DisconnectNetworkDrive(char driveLetter, bool force)
        {
            if (driveLetter < 'A' || driveLetter > 'Z')
                throw new ArgumentOutOfRangeException(nameof(driveLetter));

            UnsafeNativeApis.WNetCancelConnection2($"{driveLetter}:", 0, force);
        }
    }
}
