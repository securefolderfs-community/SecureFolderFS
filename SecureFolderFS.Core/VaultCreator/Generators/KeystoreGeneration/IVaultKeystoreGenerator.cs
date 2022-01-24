using System.IO;

namespace SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration
{
    /// <summary>
    /// Provides module for generating the vault keystore file.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultKeystoreGenerator
    {
        /// <summary>
        /// Generates keystore file and opens a <see cref="Stream"/> to it.
        /// </summary>
        /// <param name="vaultPath">The vault path.</param>
        /// <param name="keystoreFileName">Possible filename of the vault keystore file.</param>
        /// <returns></returns>
        Stream GenerateVaultKeystore(string vaultPath, string keystoreFileName);
    }
}
