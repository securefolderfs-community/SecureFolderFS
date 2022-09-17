using SecureFolderFS.Shared.Helpers;
using System;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Manages instances of <see cref="IOpenCryptFile"/>.
    /// </summary>
    public interface IOpenCryptFileManager : IDisposable
    {
        /// <summary>
        /// Tries to get an <see cref="IOpenCryptFile"/> from opened files.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path of the file.</param>
        /// <returns>An instance of <see cref="IOpenCryptFile"/>. The value may be null when the file is not present in opened files list.</returns>
        IOpenCryptFile? TryGet(string ciphertextPath);

        /// <summary>
        /// Creates new instance of <see cref="IOpenCryptFile"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path pointing to the file.</param>
        /// <param name="headerBuffer">The cleartext header of the file.</param>
        /// <returns>An instance of <see cref="IOpenCryptFile"/>.</returns>
        IOpenCryptFile CreateNew(string ciphertextPath, BufferHolder headerBuffer);
    }
}
