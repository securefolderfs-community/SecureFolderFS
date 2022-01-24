using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.Storage.Implementation
{
    internal sealed class VaultStorageReceiver : IVaultStorageReceiver
    {
        private readonly IFileStreamReceiver _fileStreamReceiver;

        private readonly IPathReceiver _pathReceiver;

        private readonly IFileSystemOperations _fileSystemOperations;

        public VaultStorageReceiver(IFileStreamReceiver fileStreamReceiver, IPathReceiver pathReceiver, IFileSystemOperations fileSystemOperations)
        {
            this._fileStreamReceiver = fileStreamReceiver;
            this._pathReceiver = pathReceiver;
            this._fileSystemOperations = fileSystemOperations;
        }

        public IVaultFile OpenVaultFile(ICleartextPath cleartextPath)
        {
            return new VaultFile(_pathReceiver.FromCleartextPath(cleartextPath), _fileSystemOperations, _fileStreamReceiver);
        }

        public IVaultFile OpenVaultFile(ICiphertextPath ciphertextPath)
        {
            return new VaultFile(ciphertextPath, _fileSystemOperations, _fileStreamReceiver);
        }

        public IVaultFolder OpenVaultFolder(ICleartextPath cleartextPath)
        {
            return new VaultFolder(_pathReceiver.FromCleartextPath(cleartextPath), _fileSystemOperations);
        }

        public IVaultFolder OpenVaultFolder(ICiphertextPath ciphertextPath)
        {
            return new VaultFolder(ciphertextPath, _fileSystemOperations);
        }
    }
}
