using System;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents an unique identifier.
    /// </summary>
    public interface IUid : IEquatable<IUid>
    {
        /// <summary>
        /// Returns a string representation of the UID.
        /// </summary>
        /// <returns>UID as string.</returns>
        string? ToString();
    }
}
