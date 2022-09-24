using System;
using System.IO;
using SecureFolderFS.Core.FileSystem.Streams;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Represents an encrypting file opened on a file system.
    /// </summary>
    public interface ICryptFile : IDisposable
    {
        /// <summary>
        /// Gets the ciphertext path of the file.
        /// </summary>
        string CiphertextPath { get; }

        /// <summary>
        /// Opens a new <see cref="CleartextStream"/> on top of <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="ciphertextStream">The ciphertext stream to be wrapped by encrypting stream.</param>
        /// <returns>A new instance of <see cref="CleartextStream"/>.</returns>
        CleartextStream OpenStream(Stream ciphertextStream);
    }
}
