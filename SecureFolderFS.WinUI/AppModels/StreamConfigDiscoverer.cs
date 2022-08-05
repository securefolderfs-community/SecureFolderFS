using System.IO;
using SecureFolderFS.Core.Discoverers;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IVaultConfigurationDiscoverer"/>
    internal sealed class StreamConfigDiscoverer : IVaultConfigurationDiscoverer
    {
        private readonly Stream _foundStream;

        public StreamConfigDiscoverer(Stream foundStream)
        {
            _foundStream = foundStream;
        }

        /// <inheritdoc/>
        public Stream DiscoverVaultConfig(string vaultPath, string configFileName)
        {
            _ = vaultPath;
            _ = configFileName;

            return _foundStream;
        }
    }
}
