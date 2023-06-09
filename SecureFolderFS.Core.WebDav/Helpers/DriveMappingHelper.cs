using SecureFolderFS.Core.WebDav.UnsafeNative;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Maps a network drive.
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="remotePath"></param>
        /// <returns>Whether the drive was mapped successfully.</returns>
        public static bool MapNetworkDrive(char driveLetter, string remotePath)
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
            return UnsafeNativeApis.WNetAddConnection2(netResource, string.Empty, string.Empty, 0) == 0;
        }

        public static void DisconnectNetworkDrive(char driveLetter, bool force)
        {
            if (driveLetter < 'A' || driveLetter > 'Z')
                throw new ArgumentOutOfRangeException(nameof(driveLetter));

            UnsafeNativeApis.WNetCancelConnection2($"{driveLetter}:", 0, force);
        }
    }
}
