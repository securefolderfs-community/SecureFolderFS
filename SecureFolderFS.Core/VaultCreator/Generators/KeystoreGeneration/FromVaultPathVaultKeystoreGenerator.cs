using System.IO;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration
{
    internal sealed class FromVaultPathVaultKeystoreGenerator : IVaultKeystoreGenerator
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultKeystoreGenerator(IFileOperations fileOperations)
        {
            this._fileOperations = fileOperations;
        }

        public Stream GenerateVaultKeystore(string vaultPath, string keystoreFileName)
        {
            string keystorePath = Path.Combine(vaultPath, keystoreFileName);

            return _fileOperations.OpenFile(keystorePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
