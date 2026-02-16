using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
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
        private readonly IAsyncSerializer<Stream> _serializer;

        /// <inheritdoc/>
        public string Id => _recycleBin.Id;

        /// <inheritdoc/>
        public string Name => _recycleBin.Name;

        public VaultRecycleBin(IModifiableFolder recycleBin, IVFSRoot vfsRoot, FileSystemSpecifics specifics, IAsyncSerializer<Stream> serializer)
        {
            _recycleBin = recycleBin;
            _vfsRoot = vfsRoot;
            _specifics = specifics;
            _serializer = serializer;
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
                if (await folderPicker.PickFolderAsync(new StartingFolderOptions("ComputerFolder"), false, cancellationToken) is not IModifiableFolder destinationFolder)
                    throw new OperationCanceledException("The user did not pick destination a folder.");

                if (!destinationFolder.Id.Contains(_vfsRoot.VirtualizedRoot.Id, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The folder is outside of the virtualized storage folder.");

                // Get deepest implementation
                var deepestImplementation = (destinationFolder as IWrapper<IFolder>)?.GetDeepestWrapper().Inner as IModifiableFolder ?? destinationFolder;

                // Only return the deepest implementation if it's ciphertext
                if (deepestImplementation.Id.Contains(_specifics.ContentFolder.Id, StringComparison.OrdinalIgnoreCase))
                    return deepestImplementation;

                if (deepestImplementation is not IStorableChild plaintextChild)
                    return null;

                // Return the equivalent ciphertext implementation
                return await AbstractPathHelpers.GetCiphertextItemAsync(plaintextChild, _specifics, cancellationToken) as IModifiableFolder;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || PathHelpers.IsCoreName(item.Name))
                    continue;

                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, _recycleBin, StreamSerializer.Instance, cancellationToken);
                if (dataModel.ParentPath is null || dataModel.OriginalName is null)
                    continue;

                // Decrypt name only when using name encryption
                var plaintextName = _specifics.Security.NameCrypt is null
                    ? dataModel.OriginalName
                    : _specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(dataModel.OriginalName), dataModel.DirectoryId);

                string? plaintextParentPath = null;
                var parentStorable = await _specifics.ContentFolder.TryGetItemByRelativePathOrSelfAsync(dataModel.ParentPath, cancellationToken);
                if (parentStorable?.Id.Equals(_specifics.ContentFolder.Id, StringComparison.OrdinalIgnoreCase) ?? false)
                    plaintextParentPath = _specifics.ContentFolder.Id;
                else if (parentStorable is IStorableChild parentStorableChild)
                    plaintextParentPath = await AbstractPathHelpers.GetPlaintextPathAsync(parentStorableChild, _specifics, cancellationToken);

                yield return new RecycleBinItem(item, this)
                {
                    Id = string.IsNullOrEmpty(plaintextParentPath) || string.IsNullOrEmpty(plaintextName) ? string.Empty : $"{plaintextParentPath}/{plaintextName}",
                    Name = plaintextName ?? item.Name,
                    DeletionTimestamp = dataModel.DeletionTimestamp ?? default,
                    Size = dataModel.Size ?? -1L
                };
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (_specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get the associated configuration file
            var configurationFile = await _recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);

            // Deserialize configuration
            RecycleBinItemDataModel? itemDataModel;
            await using (var configurationStream = await configurationFile.OpenReadAsync(cancellationToken))
                itemDataModel = await _serializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);

            // Delete both items
            await _recycleBin.DeleteAsync(item, cancellationToken);
            await _recycleBin.DeleteAsync(configurationFile, cancellationToken);

            // Check if the item had any size
            if (itemDataModel is not { Size: { } size and > 0L })
                return;

            // Update occupied size
            var occupiedSize = await AbstractRecycleBinHelpers.GetOccupiedSizeAsync(_recycleBin, cancellationToken);
            var newSize = occupiedSize - size;
            await AbstractRecycleBinHelpers.SetOccupiedSizeAsync(_recycleBin, newSize, cancellationToken);
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
        public async Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return await _recycleBin.GetFolderWatcherAsync(cancellationToken);
        }
    }
}
