using System;
using System.Text;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents a password that can be cleared.
    /// </summary>
    public interface IPassword : IDisposable
    {
        /// <summary>
        /// Gets the encoding used to encode the password.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Retrieves the password as a byte array encoded with <see cref="Encoding"/>.
        /// </summary>
        /// <returns>Returns a password in bytes, the array is empty if disposed.</returns>
        byte[] GetPassword();
    }
}
