using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.UnsafeNative;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class AvaloniaApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform => "AvaloniaUI";

        /// <inheritdoc/>
        public override AppVersion GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            return new(version, Platform);
        }

        /// <inheritdoc/>
        public override Task OpenUriAsync(Uri uri)
        {
            LauncherHelpers.Launch(uri);
            return Task.CompletedTask;
        }

        public override string GetSystemVersion()
        {
            if (!OperatingSystem.IsLinux()) // TODO 
                return base.GetSystemVersion();
            
            var distribution = "Unknown";
            try
            {
                distribution = Regex.Matches(File.ReadAllText("/etc/os-release"), "^PRETTY_NAME=\"(.+)\"$", RegexOptions.Multiline)[0].Groups[1].Value;
            }
            catch { }

            var sessionType = "Unknown";
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                sessionType = "Wayland";
            else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                sessionType = "X11";

            var os = "Unknown";
            utsname utsname = new();
            if (UnsafeNativeApis.uname(ref utsname) == 0)
                os = $"{GetString(utsname.sysname)} {GetString(utsname.release)}";

            return $"OS: {os}\n" +
                   $"Distribution: {distribution}\n" +
                   // TODO This may be unknown if the user is using just a window manager without a display manager
                   $"Desktop environment: {Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "Unknown"}\n" +
                   $"Session type: {sessionType}";
        }

        private string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).Trim('\0');
        }
    }
}