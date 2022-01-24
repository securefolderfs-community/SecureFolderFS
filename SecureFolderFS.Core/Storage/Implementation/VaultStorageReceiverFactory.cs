using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Storage.Implementation
{
    internal sealed class VaultStorageReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IFileStreamReceiver _fileStreamReceiver;

        private readonly IPathReceiver _pathReceiver;

        private readonly IFileSystemOperations _fileSystemOperations;

        public VaultStorageReceiverFactory(VaultVersion vaultVersion, IFileStreamReceiver fileStreamReceiver, IPathReceiver pathReceiver, IFileSystemOperations fileSystemOperations)
        {
            this._vaultVersion = vaultVersion;
            this._fileStreamReceiver = fileStreamReceiver;
            this._pathReceiver = pathReceiver;
            this._fileSystemOperations = fileSystemOperations;
        }

        public IVaultStorageReceiver GetVaultStorageReceiver()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new VaultStorageReceiver(_fileStreamReceiver, _pathReceiver, _fileSystemOperations);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
