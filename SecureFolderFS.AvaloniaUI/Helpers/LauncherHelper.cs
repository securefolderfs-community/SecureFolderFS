using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SecureFolderFS.AvaloniaUI.Helpers
{
    internal static class LauncherHelper
    {
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

        public static void Launch(Uri uri)
        {
            Launch(uri.ToString());
        }
    }
}