using System.IO;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal sealed class UnencryptedUniformPathReceiver : BasePathReceiver, IPathReceiver
    {
        // Paths are not encrypted so we use the same raw path for every type (ciphertext == cleartext)

        public UnencryptedUniformPathReceiver(VaultPath vaultPath)
            : base(vaultPath)
        {
        }

        protected override ICiphertextPath CiphertextPathFromRawCleartextPath(string cleartextPath)
        {
            return new CiphertextPath(cleartextPath);
        }

        protected override ICleartextPath CleartextPathFromRawCiphertextPath(string ciphertextPath)
        {
            return new CleartextPath(ciphertextPath);
        }

        public override string GetCleartextFileName(string ciphertextFilePath)
        {
            return Path.GetFileName(ciphertextFilePath);
        }

        public override string GetCiphertextFileName(string cleartextFilePath)
        {
            return Path.GetFileName(cleartextFilePath);
        }

        public override void Dispose()
        {
        }
    }
}
