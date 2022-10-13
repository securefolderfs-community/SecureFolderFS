using System.IO;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    public interface IStreamsAccess
    {
        /// <summary>
        /// Opens a new cleartext stream wrapping <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path of the file.</param>
        /// <param name="ciphertextStream">The ciphertext stream to wrap by the cleartext stream.</param>
        /// <returns>If successful, returns a new instance of <see cref="CleartextStream"/>, otherwise null.</returns>
        CleartextStream? OpenCleartextStream(string ciphertextPath, Stream ciphertextStream);
    }
}
