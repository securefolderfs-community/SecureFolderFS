using SecureFolderFS.Core.FileSystem.Paths;
using System.IO;

namespace SecureFolderFS.Core.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    internal sealed class CleartextPathConverter : IPathConverter
    {
        // Paths are not encrypted so we use the same path for every return value (ciphertext == cleartext)

        /// <inheritdoc/>
        public string CiphertextRootPath { get; }

        public CleartextPathConverter(string vaultRootPath)
        {
            CiphertextRootPath = vaultRootPath;
        }

        /// <inheritdoc/>
        public string? ToCiphertext(string cleartextPath)
        {
            return cleartextPath;
        }

        /// <inheritdoc/>
        public string? ToCleartext(string ciphertextPath)
        {
            return ciphertextPath;
        }

        /// <inheritdoc/>
        public string? GetCleartextFileName(string ciphertextFilePath)
        {
            return Path.GetFileName(ciphertextFilePath);
        }

        /// <inheritdoc/>
        public string? GetCiphertextFileName(string cleartextFilePath)
        {
            return Path.GetFileName(cleartextFilePath);
        }
    }
}
