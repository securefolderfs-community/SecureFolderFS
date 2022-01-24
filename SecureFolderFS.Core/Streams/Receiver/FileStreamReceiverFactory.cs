using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal sealed class FileStreamReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly ISecurity _security;

        private readonly OpenCryptFileReceiver _openCryptFileReceiver;

        private readonly IChunkFactory _chunkFactory;

        private readonly IFileSystemOperations _fileSystemOperations;

        public FileStreamReceiverFactory(VaultVersion vaultVersion, ISecurity security, OpenCryptFileReceiver openCryptFileReceiver, IChunkFactory chunkFactory, IFileSystemOperations fileSystemOperations)
        {
            this._vaultVersion = vaultVersion;
            this._security = security;
            this._openCryptFileReceiver = openCryptFileReceiver;
            this._chunkFactory = chunkFactory;
            this._fileSystemOperations = fileSystemOperations;
        }

        public IFileStreamReceiver GetFileStreamReceiver()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FileStreamReceiver(_security, _openCryptFileReceiver, _chunkFactory, _fileSystemOperations);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
