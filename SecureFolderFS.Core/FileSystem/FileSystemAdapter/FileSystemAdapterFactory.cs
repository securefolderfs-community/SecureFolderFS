using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.VaultDataStore;
using System;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter
{
    internal sealed class FileSystemAdapterFactory
    {
        private readonly VaultVersion _vaultVersion;
        private readonly FileSystemAdapterType _fileSystemAdapterType;
        private readonly MountVolumeDataModel _mountVolumeDataModel;
        private readonly ISecurity _security;
        private readonly IFileStreamReceiver _fileStreamReceiver;
        private readonly IFileSystemOperations _fileSystemOperations;
        private readonly IPathConverter _pathConverter;
        private readonly VaultPath _vaultPath;

        public FileSystemAdapterFactory(VaultVersion vaultVersion,
            FileSystemAdapterType fileSystemAdapterType,
            MountVolumeDataModel mountVolumeDataModel,
            ISecurity security,
            IFileStreamReceiver fileStreamReceiver,
            IFileSystemOperations fileSystemOperations,
            IPathConverter pathConverter,
            VaultPath vaultPath)
        {
            _vaultVersion = vaultVersion;
            _fileSystemAdapterType = fileSystemAdapterType;
            _mountVolumeDataModel = mountVolumeDataModel;
            _security = security;
            _fileStreamReceiver = fileStreamReceiver;
            _fileSystemOperations = fileSystemOperations;
            _pathConverter = _pathConverter;
            _vaultPath = vaultPath;
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
            var handles = new HandlesManager(_fileStreamReceiver);
            var dokanOperationsCallbacksFactory = new DokanOperationsCallbacksFactory(
                _vaultVersion,
                _security,
                _pathConverter,
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
