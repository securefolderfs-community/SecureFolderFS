using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage
{
    /// <inheritdoc cref="IRecycleBinFolder"/>
    internal sealed class RecycleBinFolder : IRecycleBinFolder
    {
        private readonly IVfsRoot _vfsRoot;
        private readonly FileSystemSpecifics _specifics;
        private readonly IModifiableFolder _recycleBin;
        private readonly IAsyncSerializer<Stream> _serializer;

        /// <inheritdoc/>
        public string Id => _recycleBin.Id;

        /// <inheritdoc/>
        public string Name => _recycleBin.Name;
        
        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new RecycleBinSizeOfProperty(_recycleBin);

        public RecycleBinFolder(IModifiableFolder recycleBin, IVfsRoot vfsRoot, FileSystemSpecifics specifics, IAsyncSerializer<Stream> serializer)
        {
            _recycleBin = recycleBin;
            _vfsRoot = vfsRoot;
            _specifics = specifics;
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task RestoreItemsAsync(IEnumerable<IStorableChild> items, IFolderPicker folderPicker, CancellationToken cancellationToken = default)
        {
            if (_specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var allItems = items.Select(x => x is IRecycleBinItem recycleBinItem ? recycleBinItem.AsWrapper<IStorable>().GetWrapperAt(1).Inner : x).Cast<IStorableChild>().ToArray();
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
                if (_vfsRoot.VirtualizedRoot is MemoryFolder)
                {
                    // In edge cases where the virtualized root is unknown (i.e., an in-memory implementation)
                    // we can't get the ciphertext implementation from there, so we'll need to manually walk down the
                    // plaintext root and get the ciphertext implementation
                    var relativePlaintextFolderId = plaintextChild.Id.Replace(_vfsRoot.VirtualizedRoot.Id, string.Empty);
                    var relativePlaintextFolder = await _vfsRoot.PlaintextRoot.GetItemByRelativePathOrSelfAsync(relativePlaintextFolderId, cancellationToken);
                    if (relativePlaintextFolder is not IWrapper<IFolder> folderWrapper)
                        return null;

                    return folderWrapper.Inner as IModifiableFolder;   
                }
                else
                    return await AbstractPathHelpers.GetCiphertextItemAsync(plaintextChild, _vfsRoot.VirtualizedRoot, _specifics, cancellationToken) as IModifiableFolder;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || PathHelpers.IsCoreName(item.Name))
                    continue;

                var recycleBinItem = await SafetyHelpers.NoFailureAsync(async () =>
                {
                    var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, _recycleBin, StreamSerializer.Instance, cancellationToken);
                    if (dataModel.ParentId is null || dataModel.Name is null)
                        return null;

                    // Decrypt name and parent path
                    var plaintextName = dataModel.DecryptName(_specifics.Security) ?? dataModel.Name;
                    var plaintextParentId = dataModel.DecryptParentId(_specifics.Security) ?? dataModel.ParentId;

                    IStorable plaintextItem = item switch
                    {
                        IFile ciphertextFile => new CryptoFile($"/{plaintextName}", ciphertextFile, _specifics),
                        IFolder ciphertextFolder => new CryptoFolder($"/{plaintextName}", ciphertextFolder, _specifics),
                        _ => throw new ArgumentOutOfRangeException(nameof(item))
                    };
                    
                    return new RecycleBinItem(plaintextItem, dataModel, this)
                    {
                        Id = string.IsNullOrEmpty(plaintextParentId) || string.IsNullOrEmpty(plaintextName) ? string.Empty : Path.Combine(plaintextParentId, plaintextName),
                        Name = plaintextName ?? item.Name
                    };
                });
                
                if (recycleBinItem is not null)
                    yield return recycleBinItem;
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
