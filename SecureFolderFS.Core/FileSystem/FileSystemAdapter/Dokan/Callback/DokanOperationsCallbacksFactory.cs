using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.VaultDataStore;
using System;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal sealed class DokanOperationsCallbacksFactory
    {
        private readonly VaultVersion _vaultVersion;
        private readonly ISecurity _security;
        private readonly IPathConverter _pathConverter;
        private readonly IFileSystemOperations _fileSystemOperations;
        private readonly MountVolumeDataModel _mountVolumeDataModel;
        private readonly VaultPath _vaultPath;
        private readonly HandlesManager _handles;

        public DokanOperationsCallbacksFactory(
            VaultVersion vaultVersion,
            ISecurity security,
            IPathConverter pathConverter,
            IFileSystemOperations fileSystemOperations,
            VaultPath vaultPath,
            MountVolumeDataModel mountVolumeDataModel,
            HandlesManager handles)
        {
            _vaultVersion = vaultVersion;
            _security = security;
            _pathConverter = pathConverter;
            _fileSystemOperations = fileSystemOperations;
            _vaultPath = vaultPath;
            _mountVolumeDataModel = mountVolumeDataModel;
            _handles = handles;
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
                forVersion1: () => new CreateFileCallback(_fileSystemOperations, _vaultPath, _pathConverter, _handles));
        }

        public ICleanupCallback GetCleanupCallback()
        {
            return ForVersion<ICleanupCallback>(
                forVersion1: () => new CleanupCallback(_fileSystemOperations, _vaultPath, _pathConverter, _handles));
        }

        public ICloseFileCallback GetCloseFileCallback()
        {
            return ForVersion<ICloseFileCallback>(
                forVersion1: () => new CloseFileCallback(_handles));
        }

        public IReadFileCallback GetReadFileCallback()
        {
            return ForVersion<IReadFileCallback>(
                forVersion1: () => new ReadFileCallback(_vaultPath, _pathConverter, _handles));
        }

        public IWriteFileCallback GetWriteFileCallback()
        {
            return ForVersion<IWriteFileCallback>(
                forVersion1: () => new WriteFileCallback(_vaultPath, _pathConverter, _handles));
        }

        public IFlushFileBuffersCallback GetFlushFileBuffersCallback()
        {
            return ForVersion<IFlushFileBuffersCallback>(
                forVersion1: () => new FlushFileBuffersCallback(_handles));
        }

        public IGetFileInformationCallback GetGetFileInformationCallback()
        {
            return ForVersion<IGetFileInformationCallback>(
                forVersion1: () => new GetFileInformationCallback(_security, _vaultPath, _pathConverter, _handles));
        }

        public IFindFilesCallback GetFindFilesCallback(IFindFilesWithPatternCallback findFilesWithPatternCallback)
        {
            return ForVersion<IFindFilesCallback>(
                forVersion1: () => new FindFilesCallback(findFilesWithPatternCallback, _handles));
        }

        public IFindFilesWithPatternCallback GetFindFilesWithPatternCallback()
        {
            return ForVersion<IFindFilesWithPatternCallback>(
                forVersion1: () => new FindFilesWithPatternCallback(_security, _vaultPath, _pathConverter, _handles));
        }

        public ISetFileAttributesCallback GetSetFileAttributesCallback()
        {
            return ForVersion<ISetFileAttributesCallback>(
                forVersion1: () => new SetFileAttributesCallback(_vaultPath, _pathConverter, _handles));
        }

        public ISetFileTimeCallback GetSetFileTimeCallback()
        {
            return ForVersion<ISetFileTimeCallback>(
                forVersion1: () => new SetFileTimeCallback(_vaultPath, _pathConverter, _handles));
        }

        public IDeleteFileCallback GetDeleteFileCallback()
        {
            return ForVersion<IDeleteFileCallback>(
                forVersion1: () => new DeleteFileCallback( _vaultPath, _pathConverter, _handles));
        }

        public IDeleteDirectoryCallback GetDeleteDirectoryCallback()
        {
            return ForVersion<IDeleteDirectoryCallback>(
                forVersion1: () => new DeleteDirectoryCallback(_fileSystemOperations, _vaultPath, _pathConverter, _handles));
        }

        public IMoveFileCallback GetMoveFileCallback()
        {
            return ForVersion<IMoveFileCallback>(
                forVersion1: () => new MoveFileCallback(_vaultPath, _pathConverter, _handles));
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
                forVersion1: () => new GetDiskFreeSpaceCallback(_vaultPath, _pathConverter, _handles));
        }

        public IGetVolumeInformationCallback GetGetVolumeInformationCallback()
        {
            return ForVersion<IGetVolumeInformationCallback>(
                forVersion1: () => new GetVolumeInformationCallback(_mountVolumeDataModel, _handles));
        }

        public IGetFileSecurityCallback GetGetFileSecurityCallback()
        {
            return ForVersion<IGetFileSecurityCallback>(
                forVersion1: () => new GetFileSecurityCallback(_vaultPath, _pathConverter, _handles));
        }

        public ISetFileSecurityCallback GetSetFileSecurityCallback()
        {
            return ForVersion<ISetFileSecurityCallback>(
                forVersion1: () => new SetFileSecurityCallback(_vaultPath, _pathConverter, _handles));
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
                forVersion1: () => new FindStreamsCallback(_vaultPath, _pathConverter, _handles));
        }
    }
}
