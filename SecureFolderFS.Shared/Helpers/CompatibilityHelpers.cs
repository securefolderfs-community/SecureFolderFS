using System.Runtime.InteropServices;

namespace SecureFolderFS.Shared.Helpers
{
    public static class CompatibilityHelpers
    {
        public static bool IsPlatformWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsPlatformOSX { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsPlatformLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
