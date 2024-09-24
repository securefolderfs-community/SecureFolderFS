using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Paths;
using System;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface IStorageRoutine : IContractRoutine
    {
        [Obsolete("Use FileSystemSpecifics")]
        IPathConverter CreateStorageComponents(IFolder contentRoot, FileSystemOptions options);

        FileSystemSpecifics GetSpecifics(IFolder contentFolder, FileSystemOptions options);
    }
}
