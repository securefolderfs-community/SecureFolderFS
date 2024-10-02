using System;
using System.IO;
using System.Linq;
using System.Text;
using SecureFolderFS.Core.WebDav.UnsafeNative;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class DriveMappingHelpers
    {
        public static string GetRemotePath(
            string protocol,
            string domain,
            int port,
            string name)
        {
            var sep = Path.DirectorySeparatorChar;
            if (OperatingSystem.IsWindows())
            {
                return $"{sep}{sep}{domain}@{port}{sep}{name}{sep}";
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                return $"{protocol}:{sep}{sep}{domain}:{port}{sep}{name}";
            }
            else if (OperatingSystem.IsLinux())
            {
                return $"{protocol}:{sep}{sep}{domain}:{port}{sep}{name}";
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Attempts to map a network drive. Doesn't throw on failure.
        /// </summary>
        public static async Task MapNetworkDriveAsync(
            string mountPath,
            string remotePath,
            CancellationToken cancellationToken = default)
        {
            if (OperatingSystem.IsWindows())
            {
                var netResource = new NETRESOURCE()
                {
                    dwType = UnsafeNativeApis.RESOURCETYPE_DISK,
                    lpLocalName = mountPath,
                    lpRemoteName = remotePath,
                };

                // WNetAddConnection2 doesn't return until it has either successfully established a connection or timed out,
                // so it has to be run in another thread to prevent blocking the server from responding.
                _ = Task.Run(() => UnsafeNativeApis.WNetAddConnection2(netResource, null!, null!, UnsafeNativeApis.CONNECT_TEMPORARY), cancellationToken);
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                var args = $"mount_webdav {remotePath} {mountPath}";
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
                await process.WaitForExitAsync(cancellationToken);
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                _ = error;
            }
        }

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
