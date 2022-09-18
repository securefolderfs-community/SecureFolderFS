using SecureFolderFS.Core.FileSystem.Paths;

namespace SecureFolderFS.Core.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    internal abstract class BasePathConverter : IPathConverter
    {
        protected readonly string vaultRootPath;

        protected BasePathConverter(string vaultRootPath)
        {
            this.vaultRootPath = vaultRootPath;
        }

        /// <inheritdoc/>
        public abstract string? ToCiphertext(string cleartextPath);

        /// <inheritdoc/>
        public abstract string? ToCleartext(string ciphertextPath);

        /// <inheritdoc/>
        public abstract string? GetCleartextFileName(string cleartextFilePath);
    }
}
