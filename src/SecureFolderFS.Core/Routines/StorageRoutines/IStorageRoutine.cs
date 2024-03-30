using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    // TODO: Needs docs
    public interface IStorageRoutine
    {
        IStorageRoutine SetUnlockContract(IDisposable unlockContract);
        
        (DirectoryIdCache, Security, IPathConverter, IStreamsAccess) CreateStorageComponents(IFolder contentRoot, FileSystemOptions options);
    }
}
