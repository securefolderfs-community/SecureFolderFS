using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal sealed class FileSystemOperationsFactory
    {
        private readonly VaultVersion _vaultVersion;
        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        public FileSystemOperationsFactory(VaultVersion vaultVersion, IDirectoryIdReceiver directoryIdReceiver)
        {
            _vaultVersion = vaultVersion;
            _directoryIdReceiver = directoryIdReceiver;
        }

        public IFileSystemOperations GetFileSystemOperations()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new FileSystemOperations(_directoryIdReceiver);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
