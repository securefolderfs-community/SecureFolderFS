using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS
using System.Linq;
using System.Text;
using SecureFolderFS.Uno.PInvoke;
#endif

namespace SecureFolderFS.Uno.Helpers
{
    public static class DriveMappingHelpers
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
            else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
            {
                return $"{protocol}:{sep}{sep}{domain}:{port}{sep}{name}";
            }
            else if (OperatingSystem.IsLinux())
            {
                return $"{protocol}:{sep}{sep}{domain}:{port}{sep}{name}";
            }

            throw new PlatformNotSupportedException();
        }

        public static async Task<string?> GetMountPathForRemotePathAsync(string remotePath)
        {
#if WINDOWS
            remotePath = remotePath.TrimEnd('\\');

            var bufferSize = remotePath.Length + 1; // Null-terminated
            var driveRemotePathBuilder = new StringBuilder(bufferSize);

            foreach (var drive in DriveInfo.GetDrives().Select(item => $"{item.Name[0]}:"))
            {
                if (UnsafeNative.WNetGetConnection(drive, driveRemotePathBuilder, ref bufferSize) == 0 && driveRemotePathBuilder.ToString() == remotePath)
                    return drive;

                driveRemotePathBuilder.Clear();
            }
#else
            if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
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
#endif

            return null;
        }

        /// <summary>
        /// Attempts to map a network drive. Doesn't throw on failure.
        /// </summary>
        public static async Task MapNetworkDriveAsync(
            string mountPath,
            string remotePath,
            CancellationToken cancellationToken = default)
        {
#if WINDOWS
            var netResource = new NETRESOURCE()
            {
                dwType = UnsafeNative.RESOURCETYPE_DISK,
                lpLocalName = mountPath,
                lpRemoteName = remotePath,
            };

            // WNetAddConnection2 doesn't return until it has either successfully established a connection or timed out,
            // so it has to be run in another thread to prevent blocking the server from responding.
            _ = Task.Run(() =>
            {
                var error = UnsafeNative.WNetAddConnection2(netResource, null!, null!, UnsafeNative.CONNECT_TEMPORARY);
                _ = error;
            }, cancellationToken);
#else
            if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
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
#endif
        }
    }
}
