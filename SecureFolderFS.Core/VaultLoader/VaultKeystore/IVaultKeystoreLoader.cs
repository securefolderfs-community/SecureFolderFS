using System.IO;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;

namespace SecureFolderFS.Core.VaultLoader.VaultKeystore
{
    /// <summary>
    /// Provides module for loading vault keystore.
    /// </summary>
    internal interface IVaultKeystoreLoader
    {
        /// <summary>
        /// Loads <see cref="BaseVaultKeystore"/> from provided <paramref name="keystoreFileStream"/>.
        /// </summary>
        /// <param name="keystoreFileStream">The <see cref="Stream"/> to keystore file.</param>
        /// <returns></returns>
        BaseVaultKeystore LoadVaultKeystore(Stream keystoreFileStream);
    }
}
