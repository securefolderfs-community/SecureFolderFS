using System;
using System.Text;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents a password that can be cleared.
    /// </summary>
    public interface IPassword : IDisposable
    {
        /// <summary>
        /// Retrieves the password as a byte array encoded with <paramref name="encoding"/>.
        /// </summary>
        /// <param name="encoding">The encoding used to encode the password.</param>
        /// <returns>Returns a password in bytes.</returns>
        byte[] GetRepresentation(Encoding encoding);

        /// <summary>
        /// Gets the password as a sequence of characters.
        /// </summary>
        /// <remarks>
        /// Prefer to use <see cref="GetRepresentation"/> over <see cref="ToString"/>
        /// to avoid leaving traces of the password in memory.
        /// </remarks>
        /// <returns>A password as a <see cref="string"/>.</returns>
        string ToString();
    }
}
