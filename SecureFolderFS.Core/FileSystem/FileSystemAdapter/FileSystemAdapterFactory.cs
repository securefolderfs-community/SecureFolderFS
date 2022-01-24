using System;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter
{
    internal sealed class FileSystemAdapterFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly FileSystemAdapterType _fileSystemAdapterType;

        private readonly MountVolumeDataModel _mountVolumeDataModel;

        private readonly IContentCryptor _contentCryptor;

        private readonly IFileSystemOperations _fileSystemOperations;

        private readonly IPathReceiver _pathReceier;

        private readonly IVaultStorageReceiver _vaultStorageReceiver;

        private readonly IStorageEnumerator _storageEnumerator;

        private readonly VaultPath _vaultPath;

        public FileSystemAdapterFactory(VaultVersion vaultVersion,
            FileSystemAdapterType fileSystemAdapterType,
            MountVolumeDataModel mountVolumeDataModel,
            IContentCryptor contentCryptor,
            IFileSystemOperations fileSystemOperations,
            IPathReceiver pathReceier,
            IVaultStorageReceiver vaultStorageReceiver,
            IStorageEnumerator storageEnumerator,
            VaultPath vaultPath)
        {
            this._vaultVersion = vaultVersion;
            this._fileSystemAdapterType = fileSystemAdapterType;
            this._mountVolumeDataModel = mountVolumeDataModel;
            this._contentCryptor = contentCryptor;
            this._fileSystemOperations = fileSystemOperations;
            this._pathReceier = pathReceier;
            this._vaultStorageReceiver = vaultStorageReceiver;
            this._storageEnumerator = storageEnumerator;
            this._vaultPath = vaultPath;
        }

        public IFileSystemAdapter GetFileSystemAdapter()
        {
            switch (_fileSystemAdapterType)
            {
                case FileSystemAdapterType.DokanAdapter:
                    return GetDokanFileSystemAdapter();

                default:
                    throw new ArgumentOutOfRangeException(nameof(_fileSystemAdapterType));
            }
        }

        public DokanFileSystemAdapter GetDokanFileSystemAdapter()
        {
            var handles = new HandlesCollection(_vaultStorageReceiver);
            var dokanOperationsCallbacksFactory = new DokanOperationsCallbacksFactory(
                _vaultVersion,
                _contentCryptor,
                _storageEnumerator,
                _pathReceier,
                _fileSystemOperations,
                _vaultPath,
                _mountVolumeDataModel,
                handles);

            var findFilesWithPatternCallback = dokanOperationsCallbacksFactory.GetFindFilesWithPatternCallback();
            var setEndOfFileCallback = dokanOperationsCallbacksFactory.GetSetEndOfFileCallback();

            return new DokanFileSystemAdapter()
            {
                CreateFileCallback = dokanOperationsCallbacksFactory.GetCreateFileCallback(),
                CleanupCallback = dokanOperationsCallbacksFactory.GetCleanupCallback(),
                CloseFileCallback = dokanOperationsCallbacksFactory.GetCloseFileCallback(),
                ReadFileCallback = dokanOperationsCallbacksFactory.GetReadFileCallback(),
                WriteFileCallback = dokanOperationsCallbacksFactory.GetWriteFileCallback(),
                FlushFileBuffersCallback = dokanOperationsCallbacksFactory.GetFlushFileBuffersCallback(),
                GetFileInformationCallback = dokanOperationsCallbacksFactory.GetGetFileInformationCallback(),
                FindFilesCallback = dokanOperationsCallbacksFactory.GetFindFilesCallback(findFilesWithPatternCallback),
                FindFilesWithPatternCallback = findFilesWithPatternCallback,
                SetFileAttributesCallback = dokanOperationsCallbacksFactory.GetSetFileAttributesCallback(),
                SetFileTimeCallback = dokanOperationsCallbacksFactory.GetSetFileTimeCallback(),
                DeleteFileCallback = dokanOperationsCallbacksFactory.GetDeleteFileCallback(),
                DeleteDirectoryCallback = dokanOperationsCallbacksFactory.GetDeleteDirectoryCallback(),
                MoveFileCallback = dokanOperationsCallbacksFactory.GetMoveFileCallback(),
                SetEndOfFileCallback = setEndOfFileCallback,
                SetAllocationSizeCallback = dokanOperationsCallbacksFactory.GetSetAllocationSizeCallback(setEndOfFileCallback),
                LockFileCallback = dokanOperationsCallbacksFactory.GetLockFileCallback(),
                UnlockFileCallback = dokanOperationsCallbacksFactory.GetUnlockFileCallback(),
                GetDiskFreeSpaceCallback = dokanOperationsCallbacksFactory.GetGetDiskFreeSpaceCallback(),
                GetVolumeInformationCallback = dokanOperationsCallbacksFactory.GetGetVolumeInformationCallback(),
                GetFileSecurityCallback = dokanOperationsCallbacksFactory.GetGetFileSecurityCallback(),
                SetFileSecurityCallback = dokanOperationsCallbacksFactory.GetSetFileSecurityCallback(),
                MountedCallback = dokanOperationsCallbacksFactory.GetMountedCallback(),
                UnmountedCallback = dokanOperationsCallbacksFactory.GetUnmountedCallback(),
                FindStreamsCallback = dokanOperationsCallbacksFactory.GetFindStreamsCallback()
            };
        }
    }
}
