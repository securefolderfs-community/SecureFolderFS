using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IRecycleBinFolder"/>
    internal sealed class VaultRecycleBin : IRecycleBinFolder
    {
        private readonly IVFSRoot _vfsRoot;
        private readonly FileSystemSpecifics _specifics;
        private readonly IModifiableFolder _recycleBin;

        /// <inheritdoc/>
        public string Id => _recycleBin.Id;

        /// <inheritdoc/>
        public string Name => _recycleBin.Name;

        public VaultRecycleBin(IModifiableFolder recycleBin, IVFSRoot vfsRoot, FileSystemSpecifics specifics)
        {
            _recycleBin = recycleBin;
            _vfsRoot = vfsRoot;
            _specifics = specifics;
        }

        /// <inheritdoc/>
        public Task<IBasicProperties> GetPropertiesAsync()
        {
            return Task.FromResult<IBasicProperties>(new RecycleBinProperties(_recycleBin));
        }

        /// <inheritdoc/>
        public async Task RestoreItemsAsync(IEnumerable<IStorableChild> items, IFolderPicker folderPicker, CancellationToken cancellationToken = default)
        {
            if (_specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var allItems = items.Select(x => x is IRecycleBinItem recycleBinItem ? recycleBinItem.Inner : x).ToArray();
            switch (allItems.Length)
            {
                case 1:
                {
                    var item = allItems[0];
                    var destinationFolder = await AbstractRecycleBinHelpers.GetDestinationFolderAsync(
                        item,
                        _specifics,
                        StreamSerializer.Instance,
                        cancellationToken);

                    // Prompt the user to pick the folder when the default destination couldn't be used
                    destinationFolder ??= await GetDestinationFolderAsync();
                    if (destinationFolder is null)
                        throw new InvalidOperationException("The destination folder couldn't be chosen.");

                    // Restore the item to chosen destination
                    await AbstractRecycleBinHelpers.RestoreAsync(
                        item,
                        destinationFolder,
                        _specifics,
                        StreamSerializer.Instance,
                        cancellationToken);

                    break;
                }

                case > 1:
                {
                    var destinationFolder = await GetDestinationFolderAsync();
                    if (destinationFolder is null)
                        throw new InvalidOperationException("The destination folder couldn't be chosen.");

                    foreach (var item in allItems)
                    {
                        await AbstractRecycleBinHelpers.RestoreAsync(
                            item,
                            destinationFolder,
                            _specifics,
                            StreamSerializer.Instance,
                            cancellationToken);
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException("Sequence contains no elements.");
            }

            return;

            async Task<IModifiableFolder?> GetDestinationFolderAsync()
            {
                // TODO: Add starting directory parameter
                var destinationFolder = await folderPicker.PickFolderAsync(null, false, cancellationToken) as IModifiableFolder;
                if (destinationFolder is null)
                    throw new OperationCanceledException("The user did not pick destination a folder.");

                // TODO: Check if the destinationFolder is outside the virtualized root, in which case, throw an exception.
                if (!destinationFolder.Id.Contains(_vfsRoot.VirtualizedRoot.Id, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The folder is outside of the virtualized storage folder.");

                var deepestWrapper = (destinationFolder as IWrapper<IFolder>)?.GetDeepestWrapper();
                if (deepestWrapper is null)
                    throw new NotSupportedException("Could not retrieve inner ciphertext item from the picked folder.");

                return deepestWrapper.Inner as IModifiableFolder;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, _recycleBin, StreamSerializer.Instance, cancellationToken);
                if (dataModel.ParentPath is null || dataModel.OriginalName is null)
                    continue;

                // Decrypt name only when using name encryption
                var plaintextName = _specifics.Security.NameCrypt is null
                    ? dataModel.OriginalName
                    : _specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(dataModel.OriginalName), dataModel.DirectoryId);

                yield return new RecycleBinItem(item, dataModel.DeletionTimestamp ?? default, plaintextName, this);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (_specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get the associated configuration file
            var configurationFile = await _recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);

            // Delete both items
            await _recycleBin.DeleteAsync(item, cancellationToken);
            await _recycleBin.DeleteAsync(configurationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IChildFolder>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IChildFile>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IFolderWatcher>(new NotSupportedException());
        }
    }
}
