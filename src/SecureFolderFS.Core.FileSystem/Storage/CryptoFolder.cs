using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    // TODO(ns): Add move and copy support
    public class CryptoFolder : CryptoStorable<IFolder>, IChildFolder, IModifiableFolder, IGetItem, IGetItemRecursive, IGetFirstByName
    {
        public CryptoFolder(IFolder inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache)
            : base(inner, streamsAccess, pathConverter, directoryIdCache)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(type, cancellationToken))
            {
                if (PathHelpers.IsCoreFile(item.Name))
                    continue;

                yield return item switch
                {
                    IFile file => (IStorableChild)Wrap(file),
                    IFolder folder => (IStorableChild)Wrap(folder),
                    _ => throw new InvalidOperationException("The enumerated item was neither a file nor a folder.")
                };
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!id.Contains(Id))
                throw new FileNotFoundException("The provided Id does not belong to an item in this folder.");

            var ciphertextId = EncryptPath(id);
            return await Inner.GetItemRecursiveAsync(ciphertextId, cancellationToken) switch
            {
                IChildFile file => (IStorableChild)Wrap(file),
                IChildFolder folder => (IStorableChild)Wrap(folder),
                _ => throw new InvalidCastException("Could not match the item to neither a file nor a folder.")
            };
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var encryptedName = EncryptName(name);
            return await Inner.GetFirstByNameAsync(encryptedName, cancellationToken) switch
            {
                IChildFile file => (IStorableChild)Wrap(file),
                IChildFolder folder => (IStorableChild)Wrap(folder),
                _ => throw new InvalidCastException("Could not match the item to neither a file nor a folder.")
            };
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!id.Contains(Id))
                throw new FileNotFoundException("The provided Id does not belong to an item in this folder.");

            var ciphertextId = EncryptPath(id);
            return await Inner.GetItemAsync(ciphertextId, cancellationToken) switch
            {
                IChildFile file => (IStorableChild)Wrap(file),
                IChildFolder folder => (IStorableChild)Wrap(folder),
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

            // TODO: Invalidate cache on success
            return modifiableFolder.DeleteAsync(item, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = EncryptName(name);
            var folder = await modifiableFolder.CreateFolderAsync(encryptedName, overwrite, cancellationToken);
            if (folder is not IModifiableFolder createdModifiableFolder)
                throw new ArgumentException("The created folder is not modifiable.");

            // Get the DirectoryID file
            var dirIdFile = await createdModifiableFolder.CreateFileAsync(FileSystem.Constants.DIRECTORY_ID_FILENAME, false, cancellationToken);
            var directoryId = Guid.NewGuid().ToByteArray();

            // Initialize directory with DirectoryID
            await using var directoryIdStream = await dirIdFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            await directoryIdStream.WriteAsync(directoryId, cancellationToken);

            // Set DirectoryID to known IDs
            directoryIdCache.SetDirectoryId(dirIdFile.Id, Guid.NewGuid().ToByteArray());

            return (IChildFolder)Wrap(folder);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = EncryptName(name);
            var file = await modifiableFolder.CreateFileAsync(encryptedName, overwrite, cancellationToken);

            return (IChildFile)Wrap(file);
        }
    }
}
