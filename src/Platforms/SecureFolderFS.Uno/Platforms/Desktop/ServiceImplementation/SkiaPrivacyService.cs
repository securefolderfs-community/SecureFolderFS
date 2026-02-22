using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IPrivacyService"/>
    internal sealed class SkiaPrivacyService : IPrivacyService
    {
        /// <inheritdoc/>
        public async Task<bool> ClearTracesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return await ClearMacOSTracesAsync(cancellationToken);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return await ClearLinuxTracesAsync(cancellationToken);

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Clears recent files and traces on macOS.
        /// </summary>
        private static async Task<bool> ClearMacOSTracesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Clear the LSSharedFileList database (stores recent documents)
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var recentItemsPath =
                    Path.Combine(homeDir, "Library", "Application Support", "com.apple.sharedfilelist");

                if (Directory.Exists(recentItemsPath))
                {
                    // Clear recent documents SFL files
                    var sflFiles = new[]
                    {
                        "com.apple.LSSharedFileList.RecentDocuments.sfl2",
                        "com.apple.LSSharedFileList.RecentDocuments.sfl3"
                    };

                    foreach (var sflFile in sflFiles)
                    {
                        var filePath = Path.Combine(recentItemsPath, sflFile);
                        if (!File.Exists(filePath))
                            continue;

                        try
                        {
                            File.Delete(filePath);
                        }
                        catch
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                }

                // Clear Finder recent folders using defaults command
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/zsh",
                    Arguments = "-c \"defaults delete com.apple.finder FXRecentFolders 2>/dev/null || true\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clears recent files and traces on Linux.
        /// </summary>
        private static async Task<bool> ClearLinuxTracesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // Clear recently-used.xbel (freedesktop.org standard)
                // This is used by GTK applications, GNOME, and other desktop environments
                var recentlyUsedPath = Path.Combine(homeDir, ".local", "share", "recently-used.xbel");
                if (File.Exists(recentlyUsedPath))
                {
                    // Write empty XML structure instead of deleting
                    var emptyXbel = """
                                    <?xml version="1.0" encoding="UTF-8"?>
                                    <xbel version="1.0"
                                          xmlns:bookmark="http://www.freedesktop.org/standards/desktop-bookmarks"
                                          xmlns:mime="http://www.freedesktop.org/standards/shared-mime-info">
                                    </xbel>
                                    """;

                    await File.WriteAllTextAsync(recentlyUsedPath, emptyXbel, cancellationToken);
                }

                // Clear Tracker database (used by GNOME for file indexing)
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"tracker3 reset -s 2>/dev/null || tracker reset -s 2>/dev/null || true\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                // Clear KDE recent documents (if using KDE)
                var kdeRecentPath = Path.Combine(homeDir, ".local", "share", "RecentDocuments");
                if (!Directory.Exists(kdeRecentPath))
                    return true;

                foreach (var file in Directory.GetFiles(kdeRecentPath, "*.desktop"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore individual file deletion errors
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
