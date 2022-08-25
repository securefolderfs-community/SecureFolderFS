using SecureFolderFS.Core.Chunks.ChunkAccessImpl;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileNames.Factory;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter;
using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Instance;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers;
using SecureFolderFS.Core.Paths.Receivers;
using SecureFolderFS.Core.Streams.Receiver;

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

            var chunkReceiverFactory = new ChunkAccessFactory(_vaultInstance.Security, _vaultInstance.VaultVersion, _optionalVaultLoadRoutine.ChunkCachingStrategy, _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker);
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
                _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations);
            _vaultInstance.SecureFolderFSInstanceImpl.FileStreamReceiver = fileStreamReceiverFactory.GetFileStreamReceiver();

            var fileSystemAdapterFactory = new FileSystemAdapterFactory(
                _vaultInstance.VaultVersion,
                _optionalVaultLoadRoutine.FileSystemAdapterType,
                _optionalVaultLoadRoutine.MountVolumeDataModel,
                _vaultInstance.Security,
                _vaultInstance.SecureFolderFSInstanceImpl.FileStreamReceiver,
                _vaultInstance.SecureFolderFSInstanceImpl.FileSystemOperations,
                _vaultInstance.SecureFolderFSInstanceImpl.PathReceiver,
                _vaultInstance.VaultPath);

            _vaultInstance.SecureFolderFSInstanceImpl.FileSystemAdapter = fileSystemAdapterFactory.GetFileSystemAdapter();

            _vaultLoadDataModel.Cleanup();

            return _vaultInstance;
        }
    }
}
