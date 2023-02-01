using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SecureFolderFS.AvaloniaUI.Helpers
{
    internal static class LauncherHelper
    {
        /// <summary>
        /// Launches the specified URL.
        /// </summary>
        /// <param name="url">A file, folder or a website.</param>
        public static void Launch(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        /// <summary>
        /// Launches the specified URI.
        /// </summary>
        /// <param name="uri">A file, folder or a website.</param>
        public static void Launch(Uri uri)
        {
            Launch(uri.ToString());
        }
    }
}