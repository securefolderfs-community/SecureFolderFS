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

        public static async Task<string?> GetMountPathForRemotePathAsync(string remotePath, CancellationToken cancellationToken = default)
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
                var startInfo = new ProcessStartInfo("/sbin/mount")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                startInfo.ArgumentList.Add("-t");
                startInfo.ArgumentList.Add("webdav");

                using var process = Process.Start(startInfo);
                if (process is null)
                    return null;

                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                foreach (var line in output.Split('\n'))
                {
                    if (TryParseMountLine(line) is { } mount && IsSameRemotePath(mount.RemotePath, remotePath))
                        return mount.MountPath;
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

        /// <summary>
        /// Reads an entry of the mount table, as listed by mount(8).
        /// </summary>
        /// <remarks>
        /// The mount point is taken whole, as it may contain spaces.
        /// </remarks>
        internal static (string RemotePath, string MountPath)? TryParseMountLine(string line)
        {
            var separator = line.IndexOf(" on ", StringComparison.Ordinal);
            var options = line.LastIndexOf(" (", StringComparison.Ordinal);
            if (separator <= 0 || options <= separator + 4)
                return null;

            return (line[..separator], line[(separator + 4)..options]);
        }

        /// <summary>
        /// Determines whether two addresses of a WebDAV share are the same, regardless of escaping and a trailing slash.
        /// </summary>
        internal static bool IsSameRemotePath(string first, string second)
        {
            return Normalize(first).Equals(Normalize(second), StringComparison.OrdinalIgnoreCase);

            static string Normalize(string remotePath) => Uri.UnescapeDataString(remotePath).TrimEnd('/');
        }
    }
}
