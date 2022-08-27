using System.IO;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal sealed class UnencryptedUniformPathReceiver : BasePathReceiver, IPathReceiver
    {
        // Paths are not encrypted so we use the same raw path for every type (ciphertext == cleartext)

        public UnencryptedUniformPathReceiver(VaultPath vaultPath)
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

        /// <inheritdoc/>
        public override string GetCiphertextFileName(string cleartextFilePath)
        {
            return Path.GetFileName(cleartextFilePath);
        }
    }
}
