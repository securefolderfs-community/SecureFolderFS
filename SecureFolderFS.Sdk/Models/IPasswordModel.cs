using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a password that can be cleared with <see cref="IDisposable"/>.
    /// </summary>
    public interface IPasswordModel : IDisposable
    {
        /// <summary>
        /// Retrieves the password as a UTF8 byte array.
        /// </summary>
        /// <returns>Returns a password in bytes, the result is null if it has been disposed.</returns>
        byte[]? GetPassword();
    }
}
