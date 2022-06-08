using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Operations.Implementation;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal sealed class FileSystemOperationsFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        private readonly IFileOperations _fileOperations;

        private readonly IDirectoryOperations _directoryOperations;

        public FileSystemOperationsFactory(VaultVersion vaultVersion, IDirectoryIdReceiver directoryIdReceiver, IFileOperations fileOperations, IDirectoryOperations directoryOperations)
        {
            _vaultVersion = vaultVersion;
            _directoryIdReceiver = directoryIdReceiver;
            _fileOperations = fileOperations;
            _directoryOperations = directoryOperations;
        }

        public IFileSystemOperations GetFileSystemOperations()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FileSystemOperations(_directoryIdReceiver, _fileOperations, _directoryOperations);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
