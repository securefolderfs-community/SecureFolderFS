using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Tunnels.Implementation
{
    internal sealed class FileSystemTunnelsFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IFileOperations _fileOperations;

        private readonly IFileStreamReceiver _fileStreamReceiver;

        private readonly IPathReceiver _pathReceiver;

        public FileSystemTunnelsFactory(VaultVersion vaultVersion, IFileOperations fileOperations, IFileStreamReceiver fileStreamReceiver, IPathReceiver pathReceiver)
        {
            this._vaultVersion = vaultVersion;
            this._fileOperations = fileOperations;
            this._fileStreamReceiver = fileStreamReceiver;
            this._pathReceiver = pathReceiver;
        }

        public IFileTunnel GetFileTunnel()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FileTunnel(_fileOperations, _fileStreamReceiver, _pathReceiver);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IFolderTunnel GetFolderTunnel()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FolderTunnel();
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
