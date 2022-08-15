using System.IO;
using SecureFolderFS.Core.Discoverers;

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
        public Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName)
        {
            _ = vaultPath;
            _ = keystoreFileName;

            return _foundStream;
        }
    }
}
