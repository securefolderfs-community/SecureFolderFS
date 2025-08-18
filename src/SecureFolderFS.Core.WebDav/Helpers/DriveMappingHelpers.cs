using SecureFolderFS.Core.WebDav.UnsafeNative;
using System;
using System.Diagnostics;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class DriveMappingHelpers
    {
        /// <summary>
        /// Attempts to disconnect a mapped network drive. Doesn't throw on failure.
        /// </summary>
        public static void DisconnectNetworkDrive(string mountPath, bool force)
        {
            if (OperatingSystem.IsWindows())
            {
                _ = UnsafeNativeApis.WNetCancelConnection2(mountPath, 0, force);
            }
            else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
            {
                Process.Start("sh", $"-c \"diskutil unmount force \"{mountPath}\"\"");
            }
        }
    }
}
