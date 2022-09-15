using System.IO;

namespace SecureFolderFS.Core.Paths
{
    internal sealed class CleartextPathConverter : BasePathConverter
    {
        // Paths are not encrypted so we use the same path for every return value (ciphertext == cleartext)

        public CleartextPathConverter(VaultPath vaultPath)
            : base(vaultPath)
        {
        }

        /// <inheritdoc/>
        public override string ToCiphertext(string cleartextPath)
        {
            return cleartextPath;
        }

        /// <inheritdoc/>
        public override string ToCleartext(string ciphertextPath)
        {
            return ciphertextPath;
        }

        /// <inheritdoc/>
        public override string GetCleartextFileName(string ciphertextFilePath)
        {
            return Path.GetFileName(ciphertextFilePath);
        }
    }
}
