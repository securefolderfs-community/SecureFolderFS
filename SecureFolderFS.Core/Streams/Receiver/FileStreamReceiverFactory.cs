using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal sealed class FileStreamReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;
        private readonly ISecurity _security;
        private readonly OpenCryptFileReceiver _openCryptFileReceiver;
        private readonly IFileSystemOperations _fileSystemOperations;

        public FileStreamReceiverFactory(VaultVersion vaultVersion, ISecurity security, OpenCryptFileReceiver openCryptFileReceiver, IFileSystemOperations fileSystemOperations)
        {
            _vaultVersion = vaultVersion;
            _security = security;
            _openCryptFileReceiver = openCryptFileReceiver;
            _fileSystemOperations = fileSystemOperations;
        }

        public IFileStreamReceiver GetFileStreamReceiver()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FileStreamReceiver(_security, _openCryptFileReceiver, _fileSystemOperations);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
