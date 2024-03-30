using System;
using System.IO;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    public interface IStreamsAccess : IDisposable
    {
        /// <summary>
        /// Opens a new cleartext stream wrapping <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <param name="ciphertextStream">The ciphertext stream to wrap by the cleartext stream.</param>
        /// <returns>If successful, returns a new instance of <see cref="CleartextStream"/>.</returns>
        Stream OpenCleartextStream(string id, Stream ciphertextStream);
    }
}
