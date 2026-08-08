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
                // Invoke diskutil directly with an argument list so the mount path is passed as a single
                // argv element and never handed to a shell. Building a "sh -c \"...\"" command string here
                // let shell metacharacters in the mount path (which derives from the attacker-influenceable
                // vault name) be parsed and executed as commands.
                var startInfo = new ProcessStartInfo
                {
                    FileName = "/usr/sbin/diskutil",
                    UseShellExecute = false
                };
                startInfo.ArgumentList.Add("unmount");
                if (force)
                    startInfo.ArgumentList.Add("force");
                startInfo.ArgumentList.Add(mountPath);

                _ = Process.Start(startInfo);
            }
        }
    }
}
