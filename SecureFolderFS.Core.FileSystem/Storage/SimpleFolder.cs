using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    // TODO: This class is mostly unimplemented. Implement: IModifiableFolder, IFolder, ILocatableFolder, and IFile, ILocatableFile for on-device and on-cloud operations
    /// <inheritdoc cref="IFolder"/>
    public sealed class SimpleFolder : ILocatableFolder
    {
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Path { get; } // Only needed for locating the vault in the UI

        public SimpleFolder(string path)
        {
            Id = path;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<INestedStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}