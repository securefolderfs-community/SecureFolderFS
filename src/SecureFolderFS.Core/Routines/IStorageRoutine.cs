using System;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface IStorageRoutine : IContractRoutine
    {
        [Obsolete]
        (DirectoryIdCache, Security, IPathConverter, StreamsAccess) CreateStorageComponents(IFolder contentRoot, FileSystemOptions options);

        FileSystemSpecifics GetSpecifics(IFolder contentFolder, FileSystemOptions options);
    }
}
