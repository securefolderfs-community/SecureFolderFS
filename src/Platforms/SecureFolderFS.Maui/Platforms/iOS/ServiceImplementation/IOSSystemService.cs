using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class IOSSystemService : ISystemService
    {
        /// <inheritdoc/>
        public event EventHandler? DesktopLocked;
    }
}
