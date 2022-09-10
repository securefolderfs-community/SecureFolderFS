using System;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Represents opened streams on a file.
    /// </summary>
    public interface IOpenCryptFile : IDisposable
    {
        /// <summary>
        /// Gets the ciphertext path of the file.
        /// </summary>
        string CiphertextPath { get; }
    }
}
