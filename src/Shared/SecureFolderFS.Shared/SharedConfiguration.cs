using System.Diagnostics;

namespace SecureFolderFS.Shared
{
    public static class SharedConfiguration
    {
        /// <summary>
        /// Enables memory-hardening features such as pinning, page locking and key XOR'ing.
        /// </summary>
        public static bool UseCoreMemoryProtection { get; set; } = true;

        /// <summary>
        /// Indicates whether the internal build label should be displayed for diagnostic or development purposes.
        /// </summary>
        public static bool ShowInternalBuildLabel { get; set; } = true;

        /// <summary>
        /// Indicates whether to use sound effects when the onboarding screen is shown.
        /// </summary>
        public static bool EnableOnboardingSfx { get; set; } = Debugger.IsAttached;
    }
}
