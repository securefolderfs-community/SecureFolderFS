using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed partial class SkiaSystemService : ISystemService
    {
        private const string AUTOSTART_ENTRY_ID = "org.securefolderfs.SecureFolderFS";

        private EventHandler? _deviceLocked;

#if !__UNO_SKIA_MACOS__
        /// <inheritdoc/>
        public event EventHandler? DeviceLocked
        {
            add => _deviceLocked += value;
            remove => _deviceLocked -= value;
        }
#endif

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            try
            {
                var drive = new DriveInfo("/");
                if (!drive.IsReady)
                    return Task.FromResult(0L);

                return Task.FromResult(drive.AvailableFreeSpace);
            }
            catch (Exception)
            {
                return Task.FromResult(0L);
            }
        }

        /// <inheritdoc/>
        public Task<bool> IsAutoStartEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (OperatingSystem.IsMacOS())
                return Task.FromResult(File.Exists(GetMacOSLaunchAgentPath()));

            if (OperatingSystem.IsLinux())
                return Task.FromResult(File.Exists(GetLinuxAutostartEntryPath()));

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<bool> TrySetAutoStartAsync(bool isEnabled, CancellationToken cancellationToken = default)
        {
            try
            {
                if (OperatingSystem.IsMacOS())
                {
                    var launchAgentPath = GetMacOSLaunchAgentPath();
                    if (!isEnabled)
                    {
                        if (File.Exists(launchAgentPath))
                            File.Delete(launchAgentPath);

                        return true;
                    }

                    var executablePath = Environment.ProcessPath;
                    if (executablePath is null)
                        return false;

                    // When running from an .app bundle, launch the bundle itself so macOS treats it as a regular app launch
                    var bundleIndex = executablePath.IndexOf(".app/Contents/MacOS/", StringComparison.Ordinal);
                    var programArguments = bundleIndex >= 0
                        ? new[] { "/usr/bin/open", "-a", executablePath[..(bundleIndex + ".app".Length)] }
                        : new[] { executablePath };

                    var argumentsXml = string.Join(Environment.NewLine, programArguments.Select(static x => $"        <string>{SecurityElement.Escape(x)}</string>"));
                    var plistContents = $"""
                        <?xml version="1.0" encoding="UTF-8"?>
                        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                        <plist version="1.0">
                        <dict>
                            <key>Label</key>
                            <string>{AUTOSTART_ENTRY_ID}</string>
                            <key>ProgramArguments</key>
                            <array>
                        {argumentsXml}
                            </array>
                            <key>RunAtLoad</key>
                            <true/>
                        </dict>
                        </plist>
                        """;

                    _ = Directory.CreateDirectory(Path.GetDirectoryName(launchAgentPath)!);
                    await File.WriteAllTextAsync(launchAgentPath, plistContents, cancellationToken);
                    return true;
                }

                if (OperatingSystem.IsLinux())
                {
                    var autostartEntryPath = GetLinuxAutostartEntryPath();
                    if (!isEnabled)
                    {
                        if (File.Exists(autostartEntryPath))
                            File.Delete(autostartEntryPath);

                        return true;
                    }

                    var executablePath = Environment.ProcessPath;
                    if (executablePath is null)
                        return false;

                    var desktopEntryContents = $"""
                        [Desktop Entry]
                        Type=Application
                        Name=SecureFolderFS
                        Exec="{executablePath}"
                        Terminal=false
                        X-GNOME-Autostart-enabled=true
                        """;

                    _ = Directory.CreateDirectory(Path.GetDirectoryName(autostartEntryPath)!);
                    await File.WriteAllTextAsync(autostartEntryPath, desktopEntryContents, cancellationToken);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetMacOSLaunchAgentPath()
        {
            var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfilePath, "Library", "LaunchAgents", $"{AUTOSTART_ENTRY_ID}.plist");
        }

        private static string GetLinuxAutostartEntryPath()
        {
            // ApplicationData resolves to $XDG_CONFIG_HOME (or ~/.config) on Linux
            var configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(configPath, "autostart", $"{AUTOSTART_ENTRY_ID}.desktop");
        }
    }
}
