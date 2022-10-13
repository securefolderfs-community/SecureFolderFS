using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany.Storage
{
    // TODO: This class is mostly unimplemented. Implement: IModifiableFolder, IFolder, ILocatableFolder, and IFile, ILocatableFile for on-device and on-cloud operations
    internal sealed class SimpleDokanyFolder : ILocatableFolder
    {
        public string Id { get; }

        public string Name { get; }

        public string Path { get; } // Only needed for locating the vault in the UI

        public SimpleDokanyFolder(string path)
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

        public IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
