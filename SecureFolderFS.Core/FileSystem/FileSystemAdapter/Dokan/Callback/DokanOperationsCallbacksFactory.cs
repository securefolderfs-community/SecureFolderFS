﻿using System;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal sealed class DokanOperationsCallbacksFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IContentCryptor _contentCryptor;

        private readonly IStorageEnumerator _storageEnumerator;

        private readonly IPathReceiver _pathReceiver;

        private readonly IFileSystemOperations _fileSystemOperations;

        private readonly MountVolumeDataModel _mountVolumeDataModel;

        private readonly VaultPath _vaultPath;

        private readonly HandlesCollection _handles;

        public DokanOperationsCallbacksFactory(
            VaultVersion vaultVersion,
            IContentCryptor contentCryptor,
            IStorageEnumerator storageEnumerator,
            IPathReceiver pathReceiver,
            IFileSystemOperations fileSystemOperations,
            VaultPath vaultPath,
            MountVolumeDataModel mountVolumeDataModel,
            HandlesCollection handles)
        {
            this._vaultVersion = vaultVersion;
            this._contentCryptor = contentCryptor;
            this._storageEnumerator = storageEnumerator;
            this._pathReceiver = pathReceiver;
            this._fileSystemOperations = fileSystemOperations;
            this._vaultPath = vaultPath;
            this._mountVolumeDataModel = mountVolumeDataModel;
            this._handles = handles;
        }

        public TCallback ForVersion<TCallback>(Func<TCallback> forVersion1, Func<TCallback> forVersion2 = null)
            where TCallback : class
        {
            ArgumentNullException.ThrowIfNull(forVersion1);

            if (_vaultVersion.SupportsVersion(VaultVersion.V1, VaultVersion.V1))
            {
                return forVersion1.Invoke();
            }
            else
            {
                Func<TCallback> lastNotNull = forVersion2 ?? forVersion1 ?? null; // ...

                if (true) // For VaultVersion.V2
                {
                    return forVersion2?.Invoke() ?? lastNotNull.Invoke();
                }
                // else if
            }
        }

        public ICreateFileCallback GetCreateFileCallback()
        {
            return ForVersion<ICreateFileCallback>(
                forVersion1: () => new CreateFileCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public ICleanupCallback GetCleanupCallback()
        {
            return ForVersion<ICleanupCallback>(
                forVersion1: () => new CleanupCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public ICloseFileCallback GetCloseFileCallback()
        {
            return ForVersion<ICloseFileCallback>(
                forVersion1: () => new CloseFileCallback(_handles));
        }

        public IReadFileCallback GetReadFileCallback()
        {
            return ForVersion<IReadFileCallback>(
                forVersion1: () => new ReadFileCallback(_vaultPath, _pathReceiver, _handles));
        }

        public IWriteFileCallback GetWriteFileCallback()
        {
            return ForVersion<IWriteFileCallback>(
                forVersion1: () => new WriteFileCallback(_vaultPath, _pathReceiver, _handles));
        }

        public IFlushFileBuffersCallback GetFlushFileBuffersCallback()
        {
            return ForVersion<IFlushFileBuffersCallback>(
                forVersion1: () => new FlushFileBuffersCallback(_handles));
        }

        public IGetFileInformationCallback GetGetFileInformationCallback()
        {
            return ForVersion<IGetFileInformationCallback>(
                forVersion1: () => new GetFileInformationCallback(_contentCryptor, _storageEnumerator, _vaultPath, _pathReceiver, _handles));
        }

        public IFindFilesCallback GetFindFilesCallback(IFindFilesWithPatternCallback findFilesWithPatternCallback)
        {
            return ForVersion<IFindFilesCallback>(
                forVersion1: () => new FindFilesCallback(findFilesWithPatternCallback, _handles));
        }

        public IFindFilesWithPatternCallback GetFindFilesWithPatternCallback()
        {
            return ForVersion<IFindFilesWithPatternCallback>(
                forVersion1: () => new FindFilesWithPatternCallback(_contentCryptor, _storageEnumerator, _vaultPath, _pathReceiver, _handles));
        }

        public ISetFileAttributesCallback GetSetFileAttributesCallback()
        {
            return ForVersion<ISetFileAttributesCallback>(
                forVersion1: () => new SetFileAttributesCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public ISetFileTimeCallback GetSetFileTimeCallback()
        {
            return ForVersion<ISetFileTimeCallback>(
                forVersion1: () => new SetFileTimeCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public IDeleteFileCallback GetDeleteFileCallback()
        {
            return ForVersion<IDeleteFileCallback>(
                forVersion1: () => new DeleteFileCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public IDeleteDirectoryCallback GetDeleteDirectoryCallback()
        {
            return ForVersion<IDeleteDirectoryCallback>(
                forVersion1: () => new DeleteDirectoryCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public IMoveFileCallback GetMoveFileCallback()
        {
            return ForVersion<IMoveFileCallback>(
                forVersion1: () => new MoveFileCallback(_fileSystemOperations, _vaultPath, _pathReceiver, _handles));
        }

        public ISetEndOfFileCallback GetSetEndOfFileCallback()
        {
            return ForVersion<ISetEndOfFileCallback>(
                forVersion1: () => new SetEndOfFileCallback(_handles));
        }

        public ISetAllocationSizeCallback GetSetAllocationSizeCallback(ISetEndOfFileCallback setEndOfFileCallback)
        {
            return ForVersion<ISetAllocationSizeCallback>(
                forVersion1: () => new SetAllocationSizeCallback(setEndOfFileCallback, _handles));
        }

        public ILockFileCallback GetLockFileCallback()
        {
            return ForVersion<ILockFileCallback>(
                forVersion1: () => new LockFileCallback(_handles));
        }

        public IUnlockFileCallback GetUnlockFileCallback()
        {
            return ForVersion<IUnlockFileCallback>(
                forVersion1: () => new UnlockFileCallback(_handles));
        }

        public IGetDiskFreeSpaceCallback GetGetDiskFreeSpaceCallback()
        {
            return ForVersion<IGetDiskFreeSpaceCallback>(
                forVersion1: () => new GetDiskFreeSpaceCallback(_vaultPath, _pathReceiver, _handles));
        }

        public IGetVolumeInformationCallback GetGetVolumeInformationCallback()
        {
            return ForVersion<IGetVolumeInformationCallback>(
                forVersion1: () => new GetVolumeInformationCallback(_mountVolumeDataModel, _handles));
        }

        public IGetFileSecurityCallback GetGetFileSecurityCallback()
        {
            return ForVersion<IGetFileSecurityCallback>(
                forVersion1: () => new GetFileSecurityCallback(_vaultPath, _pathReceiver, _handles));
        }

        public ISetFileSecurityCallback GetSetFileSecurityCallback()
        {
            return ForVersion<ISetFileSecurityCallback>(
                forVersion1: () => new SetFileSecurityCallback(_vaultPath, _pathReceiver, _handles));
        }

        public IMountedCallback GetMountedCallback()
        {
            return ForVersion<IMountedCallback>(
                forVersion1: () => new MountedCallback(_handles));
        }

        public IUnmountedCallback GetUnmountedCallback()
        {
            return ForVersion<IUnmountedCallback>(
                forVersion1: () => new UnmountedCallback(_handles));
        }

        public IFindStreamsCallback GetFindStreamsCallback()
        {
            return ForVersion<IFindStreamsCallback>(
                forVersion1: () => new FindStreamsCallback(_vaultPath, _pathReceiver, _handles));
        }
    }
}
