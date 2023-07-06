using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SecureFolderFS.AvaloniaUI.Helpers
{
    internal static class LauncherHelpers
    {
        /// <summary>
        /// Launches the specified URL.
        /// </summary>
        /// <param name="url">An URL that points to a file, folder or website.</param>
        public static void Launch(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", new[] { url });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", new[] { url });
            }
        }

        /// <summary>
        /// Launches the specified URI.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> instance that points to a file, folder or website.</param>
        public static void Launch(Uri uri)
        {
            Launch(uri.ToString());
        }
    }
}