using System;
using System.IO;

namespace SecureFolderFS.Core.Discoverers
{
    /// <summary>
    /// Provides module for discovering the vault keystore file.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    [Obsolete]
    public interface IVaultKeystoreDiscoverer
    {
        /// <summary>
        /// Opens a <see cref="Stream"/> to keystore file.
        /// </summary>
        /// <param name="vaultPath">The vault path.</param>
        /// <param name="keystoreFileName">Possible filename of the vault keystore file.</param>
        /// <returns></returns>
        Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName);
    }
}
