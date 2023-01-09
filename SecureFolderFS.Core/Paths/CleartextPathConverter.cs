using SecureFolderFS.Core.FileSystem.Paths;
using System.IO;

namespace SecureFolderFS.Core.Paths
{
    internal sealed class CleartextPathConverter : IPathConverter
    {
        // Paths are not encrypted so we use the same path for every return value (ciphertext == cleartext)

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
    }
}
