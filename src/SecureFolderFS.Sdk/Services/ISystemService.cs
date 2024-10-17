using System;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to control system related actions and events.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// Occurs when the user locks their desktop.
        /// </summary>
        event EventHandler? DesktopLocked;
    }
}
