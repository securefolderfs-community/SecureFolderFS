using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault change events.
    /// </summary>
    public sealed class VaultChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the value that determines whether the vault contents have changed.
        /// If the value is true, the validity of the vault should be reevaluated; otherwise the state of vault contents has not been changed.
        /// </summary>
        public bool ContentsChanged { get; }

        public VaultChangedEventArgs(bool contentsChanged)
        {
            ContentsChanged = contentsChanged;
        }
    }
}
