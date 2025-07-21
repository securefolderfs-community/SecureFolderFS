using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for health scanning finished notifications.
    /// </summary>
    public sealed class ScanningFinishedEventArgs(bool wasCanceled) : EventArgs
    {
        /// <summary>
        /// Gets the value that determines whether the scanning was interrupted (canceled) by the user.
        /// </summary>
        public bool WasCanceled { get; } = wasCanceled;
    }
}