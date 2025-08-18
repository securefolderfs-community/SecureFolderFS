using SecureFolderFS.Sdk.Enums;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for update change events.
    /// </summary>
    public sealed class UpdateChangedEventArgs(AppUpdateResultType updateState) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="AppUpdateResultType"/> value that represents the current state of an ongoing update.
        /// </summary>
        public AppUpdateResultType UpdateState { get; } = updateState;
    }
}
