using OwlCore.Storage;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    public sealed class CleartextPathConverter : IPathConverter
    {
        // Paths are not encrypted hence we use the same path for every return value (ciphertext == cleartext)

        /// <inheritdoc/>
        public IFolder ContentFolder { get; }

        private CleartextPathConverter(IFolder contentFolder)
        {
            ContentFolder = contentFolder;
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

        /// <summary>
        /// Creates a new instance of <see cref="IPathConverter"/>.
        /// </summary>
        /// <param name="contentFolder">The ciphertext root content folder.</param>
        /// <returns>A new instance of <see cref="IPathConverter"/>.</returns>
        public static IPathConverter CreateNew(IFolder contentFolder)
        {
            return new CleartextPathConverter(contentFolder);
        }
    }
}
