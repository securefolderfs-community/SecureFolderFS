using System;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Notifies that the state of an object has changed.
    /// </summary>
    public interface INotifyStateChanged
    {
        /// <summary>
        /// Occurs when the state changes.
        /// </summary>
        event EventHandler<EventArgs> StateChanged;
    }
}
