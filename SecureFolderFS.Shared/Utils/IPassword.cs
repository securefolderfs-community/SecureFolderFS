using System;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents a password that can be cleared.
    /// </summary>
    public interface IPassword : IDisposable
    {
        /// <summary>
        /// Retrieves the password as a UTF8 byte array.
        /// </summary>
        /// <returns>Returns a password in bytes, the result is null if it has been disposed.</returns>
        byte[] GetPassword();
    }
}
