using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface IStorageRoutine : IContractRoutine
    {
        (DirectoryIdCache, Security, IPathConverter, IStreamsAccess) CreateStorageComponents(IFolder contentRoot, FileSystemOptions options);
    }
}
