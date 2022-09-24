using SecureFolderFS.Shared.Helpers;
using System;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Manages instances of <see cref="ICryptFile"/>.
    /// </summary>
    public interface ICryptFileManager : IDisposable
    {
        /// <summary>
        /// Tries to get an <see cref="ICryptFile"/> from opened files.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path of the file.</param>
        /// <returns>An instance of <see cref="ICryptFile"/>. The value may be null when the file is not present in opened files list.</returns>
        ICryptFile? TryGet(string ciphertextPath);

        /// <summary>
        /// Creates new instance of <see cref="ICryptFile"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path pointing to the file.</param>
        /// <param name="headerBuffer">The cleartext header of the file.</param>
        /// <returns>If successful, returns an instance of <see cref="ICryptFile"/>, otherwise null.</returns>
        ICryptFile? CreateNew(string ciphertextPath, BufferHolder headerBuffer);
    }
}
