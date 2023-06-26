using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    // TODO: This class is mostly unimplemented. Implement: IModifiableFolder, IFolder, ILocatableFolder, and IFile, ILocatableFile for on-device and on-cloud operations
    public sealed class SimpleFolder : ILocatableFolder
    {
        public string Id { get; }

        public string Name { get; }

        public string Path { get; } // Only needed for locating the vault in the UI

        public SimpleFolder(string path)
        {
            Id = path;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }

        public Task<IFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<INestedStorable> GetItemsAsync(StorableKind kind = StorableKind.All, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}