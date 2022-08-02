using SecureFolderFS.Core.Instance;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.Storage.Implementation;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers;
using SecureFolderFS.Core.Tunnels.Implementation;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Paths.Receivers;
using SecureFolderFS.Core.FileNames.Factory;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation
{
    internal sealed class FinalizedVaultLoadRoutine : IFinalizedVaultLoadRoutine
    {
        private readonly VaultInstance _vaultInstance;

        private readonly VaultLoadDataModel _vaultLoadDataModel;

        private OptionalVaultLoadRoutine _optionalVaultLoadRoutine;

        public FinalizedVaultLoadRoutine(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
        {
            _vaultInstance = vaultInstance;
            _vaultLoadDataModel = vaultLoadDataModel;
        }

        public IOptionalVaultLoadRoutine ContinueWithOptionalRoutine()
        {
            _optionalVaultLoadRoutine = new OptionalVaultLoadRoutine(_vaultInstance, () => this);
            return _optionalVaultLoadRoutine;
        }

        public IVaultInstance Deploy()
        {
            if (_optionalVaultLoadRoutine is null)
            {
                OptionalVaultLoadRoutine.CreateWithDefaultOptions(this, _vaultInstance);
            }

            var chunkReceiverFactory = new ChunkReceiverFactory(_vaultInstance.VaultVersion, _vaultLoadDataModel.ChunkFactory, _optionalVaultLoadRoutine.ChunkCachingStrategy, _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker);
            var openCryptFileReceiver = new OpenCryptFileReceiver(_vaultInstance.Security, chunkReceiverFactory);
            
            var directoryIdReceiverFactory = new DirectoryIdReceiverFactory(_vaultInstance.VaultVersion, _optionalVaultLoadRoutine.DirectoryIdCachingStrategy, _vaultInstance.FileOperations, _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker);
            var directoryIdReceiver = directoryIdReceiverFactory.GetDirectoryIdReceiver(directoryIdReceiverFactory.GetDirectoryIdReader());

            var fileNameReceiverFactory = new FileNameReceiverFactory(_vaultInstance.VaultVersion, _vaultInstance.Security, _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker, _optionalVaultLoadRoutine.FileNameCachingStrategy);
            var fileNameReceiver = fileNameReceiverFactory.GetFileNameReceiver();

            var pathReceiverFactory = new PathReceiverFactory(_vaultInstance.VaultVersion, _vaultInstance.VaultPath, directoryIdReceiver, fileNameReceiver, _vaultInstance.BaseVaultConfiguration.FileNameCipherScheme);
            _vaultInstance.SecureFolderFSInstanceImpl.PathReceiver = pathReceiverFactory.GetPathReceiver();

            var fileSystemOperationsFactory = new FileSystemOperationsFactory(_vaultInstance.VaultVersion, directoryIdReceiver, _vaultInstance.FileOperations, _vaultInstance.DirectoryOperations);
            _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations = fileSystemOperationsFactory.GetFileSystemOperations();

            var fileStreamReceiverFactory = new FileStreamReceiverFactory(
                _vaultInstance.VaultVersion,
                _vaultInstance.Security,
                openCryptFileReceiver,
                _vaultLoadDataModel.ChunkFactory,
                _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations);
            _vaultInstance.SecureFolderFSInstanceImpl.FileStreamReceiver = fileStreamReceiverFactory.GetFileStreamReceiver();

            var vaultStorageReceiverFactory = new VaultStorageReceiverFactory(_vaultInstance.VaultVersion, _vaultInstance.SecureFolderFSInstanceImpl.FileStreamReceiver, _vaultInstance.SecureFolderFSInstanceImpl.PathReceiver, _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations);
            var vaultStorageReceiver = vaultStorageReceiverFactory.GetVaultStorageReceiver();
            _vaultInstance.VaultStorageReceiver = vaultStorageReceiver;

            var fileSystemTunnelsFactory = new FileSystemTunnelsFactory(_vaultInstance.VaultVersion, _vaultInstance.FileOperations, _vaultInstance.SecureFolderFSInstanceImpl.FileStreamReceiver, _vaultInstance.SecureFolderFSInstanceImpl.PathReceiver);
            _vaultInstance.FileTunnel = fileSystemTunnelsFactory.GetFileTunnel();
            _vaultInstance.FolderTunnel = fileSystemTunnelsFactory.GetFolderTunnel();

            var fileSystemAdapterFactory = new FileSystemAdapterFactory(
                _vaultInstance.VaultVersion,
                _optionalVaultLoadRoutine.FileSystemAdapterType,
                _optionalVaultLoadRoutine.MountVolumeDataModel,
                _vaultInstance.Security.ContentCryptor,
                _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations,
                _vaultInstance.SecureFolderFSInstanceImpl.PathReceiver,
                _vaultInstance.VaultStorageReceiver,
                _optionalVaultLoadRoutine.StorageEnumerator,
                _vaultInstance.VaultPath);

            _vaultInstance.SecureFolderFSInstanceImpl.FileSystemAdapter = fileSystemAdapterFactory.GetFileSystemAdapter();

            _vaultLoadDataModel.Cleanup();

            return _vaultInstance;
        }
    }
}
