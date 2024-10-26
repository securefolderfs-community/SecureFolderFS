using SecureFolderFS.Core.WebDav.UnsafeNative;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class DriveMappingHelpers
    {
        /// <summary>
        /// Attempts to disconnect a mapped network drive. Doesn't throw on failure.
        /// </summary>
        public static async Task DisconnectNetworkDriveAsync(string mountPath, bool force)
        {
            if (OperatingSystem.IsWindows())
            {
                _ = UnsafeNativeApis.WNetCancelConnection2(mountPath, 0, force);
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                var args = $"umount {mountPath}";
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{args}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
            }
        }

        public static async Task<string?> GetMountPathForRemotePathAsync(string remotePath)
        {
            if (OperatingSystem.IsWindows())
            {
                remotePath = remotePath.TrimEnd('\\');

                var bufferSize = remotePath.Length + 1; // Null-terminated
                var driveRemotePathBuilder = new StringBuilder(bufferSize);

                foreach (var drive in DriveInfo.GetDrives().Select(item => $"{item.Name[0]}:"))
                {
                    if (UnsafeNativeApis.WNetGetConnection(drive, driveRemotePathBuilder, ref bufferSize) == 0 && driveRemotePathBuilder.ToString() == remotePath)
                        return drive;

                    driveRemotePathBuilder.Clear();
                }
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "mount",
                        Arguments = "-t webdav",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(remotePath))
                    {
                        var parts = line.Split(' ');
                        if (parts.Length > 2)
                            return parts[2]; // Assuming the mount path is the third part
                    }
                }
            }

            return null;
        }
    }
}
