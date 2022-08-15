using System;

namespace SecureFolderFS.Sdk.Storage.EventArguments
{
    /// <summary>
    /// Event arguments that contains the affected storage path.
    /// </summary>
    public sealed class PathAffectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the path that has been affected.
        /// </summary>
        public string AffectedPath { get; }

        public PathAffectedEventArgs(string affectedPath)
        {
            AffectedPath = affectedPath;
        }
    }
}
