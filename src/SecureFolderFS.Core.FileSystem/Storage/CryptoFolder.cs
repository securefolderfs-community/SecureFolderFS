using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    // TODO(ns): Add move and copy support
    /// <inheritdoc cref="IFolder"/>
    public class CryptoFolder : CryptoStorable<IFolder>, IChildFolder, IModifiableFolder, IGetFirstByName
    {
        public CryptoFolder(string plaintextId, IFolder inner, FileSystemSpecifics specifics, CryptoFolder? parent = null)
            : base(plaintextId, inner, specifics, parent)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(type, cancellationToken))
            {
                if (PathHelpers.IsCoreFile(item.Name))
                    continue;

                var plaintextName = await DecryptNameAsync(item.Name, Inner);
                if (plaintextName is null)
                    continue;

                yield return item switch
                {
                    IFile file => (IStorableChild)Wrap(file, plaintextName),
                    IFolder folder => (IStorableChild)Wrap(folder, plaintextName),
                    _ => throw new InvalidOperationException("The enumerated item was neither a file nor a folder.")
                };
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var ciphertextName = await EncryptNameAsync(name, Inner);
            return await Inner.GetFirstByNameAsync(ciphertextName, cancellationToken) switch
            {
                IChildFile file => (IStorableChild)Wrap(file, name),
                IChildFolder folder => (IStorableChild)Wrap(folder, name),
                _ => throw new InvalidCastException("Could not match the item to neither a file nor a folder.")
            };
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            // TODO(ns): Implement FolderWatcher for CryptoFolder
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            // TODO: Get and delete the ciphertext item on disk, not plaintext representation
            // TODO: Invalidate cache on success
            return modifiableFolder.DeleteAsync(item, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = await EncryptNameAsync(name, Inner);
            var folder = await modifiableFolder.CreateFolderAsync(encryptedName, overwrite, cancellationToken);
            if (folder is not IModifiableFolder createdModifiableFolder)
                throw new ArgumentException("The created folder is not modifiable.");

            // Get the DirectoryID file
            var dirIdFile = await createdModifiableFolder.CreateFileAsync(FileSystem.Constants.Names.DIRECTORY_ID_FILENAME, false, cancellationToken);
            var directoryId = Guid.NewGuid().ToByteArray();

            // Initialize directory with DirectoryID
            await using var directoryIdStream = await dirIdFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            await directoryIdStream.WriteAsync(directoryId, cancellationToken);

            // Set DirectoryID to known IDs
            specifics.DirectoryIdCache.CacheSet(dirIdFile.Id, new(Guid.NewGuid().ToByteArray()));

            return (IChildFolder)Wrap(folder, name);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = await EncryptNameAsync(name, Inner);
            var file = await modifiableFolder.CreateFileAsync(encryptedName, overwrite, cancellationToken);

            return (IChildFile)Wrap(file, name);
        }
    }
}
