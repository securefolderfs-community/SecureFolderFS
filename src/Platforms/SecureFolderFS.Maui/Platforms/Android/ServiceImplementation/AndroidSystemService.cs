using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class AndroidSystemService : ISystemService
    {
        // TODO: Use BroadcastReceiver - ActionScreenOff, ActionUserPresent
        
        /// <inheritdoc/>
        public event EventHandler? DesktopLocked;
    }
}
