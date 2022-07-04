using System.IO;
using SecureFolderFS.Core.VaultLoader.Discoverers.KeystoreDiscovery;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IVaultKeystoreDiscoverer"/>
    internal sealed class StreamKeystoreDiscoverer : IVaultKeystoreDiscoverer
    {
        private readonly Stream _foundStream;

        public StreamKeystoreDiscoverer(Stream foundStream)
        {
            _foundStream = foundStream;
        }

        /// <inheritdoc/>
        public Stream OpenStreamToVaultKeystore(string vaultPath, string keystoreFileName)
        {
            _ = vaultPath;
            _ = keystoreFileName;

            return _foundStream;
        }
    }
}
