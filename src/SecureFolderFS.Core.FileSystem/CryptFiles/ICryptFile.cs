using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Represents an encrypting file opened on a file system.
    /// </summary>
    public interface ICryptFile : IDisposable
    {
        /// <summary>
        /// Gets the unique ID of the file.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Opens a new <see cref="CleartextStream"/> on top of <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="ciphertextStream">The ciphertext stream to be wrapped by encrypting stream.</param>
        /// <returns>A new instance of <see cref="CleartextStream"/>.</returns>
        CleartextStream OpenStream(Stream ciphertextStream);
    }
}
