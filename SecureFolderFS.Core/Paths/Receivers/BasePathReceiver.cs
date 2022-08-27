using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal abstract class BasePathReceiver : IPathReceiver
    {
        protected readonly VaultPath vaultPath;

        protected BasePathReceiver(VaultPath vaultPath)
        {
            this.vaultPath = vaultPath;
        }

        /// <inheritdoc/>
        public abstract string ToCiphertext(string cleartextPath);

        /// <inheritdoc/>
        public abstract string ToCleartext(string ciphertextPath);

        /// <inheritdoc/>
        public abstract string GetCleartextFileName(string cleartextFilePath);

        /// <inheritdoc/>
        public abstract string GetCiphertextFileName(string ciphertextFilePath);
    }
}
