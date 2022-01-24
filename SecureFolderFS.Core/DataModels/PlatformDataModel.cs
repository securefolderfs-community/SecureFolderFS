using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.DataModels
{
    internal static class PlatformDataModel
    {
        public static bool IsPlatformOSX { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsPlatformWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
