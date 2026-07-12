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

            var allItems = items.ToArray();
            if (allItems.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");

            // The folder picker is only shown (at most once) for items whose original location is gone
            IModifiableFolder? pickedFolder = null;
            var exceptions = new List<Exception>();

            // Read and validate every configuration file up front so that unrestorable items
            // fail immediately instead of after prompting the user for a destination
            var restoreQueue = new List<(IStorableChild Item, IStorableChild OriginalItem, string? PlaintextParentPath)>();
            foreach (var originalItem in allItems)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if ((originalItem is IRecycleBinItem unwrappable ? unwrappable.AsWrapper<IStorable>().GetWrapperAt(1).Inner : originalItem) is not IStorableChild item)
                    continue;

                try
                {
                    var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, _recycleBin, _serializer, cancellationToken);
                    var plaintextParentPath = SafetyHelpers.NoFailureResult(() => dataModel.DecryptParentId(_specifics.Security));
                    restoreQueue.Add((item, originalItem, plaintextParentPath));
                }
                catch (Exception ex)
                {
                    // A failed item must not abandon the remaining ones - aggregate and report once
                    exceptions.Add(ex);
                }
            }

            // Restore shallow destinations first. Folder trees deleted member-by-member (e.g., by
            // OS WebDav or FUSE clients, which issue one delete per item) end up as separate
            // entries; restoring parents before children reconstructs the original hierarchy
            foreach (var (item, originalItem, _) in restoreQueue.OrderBy(x => GetPathDepth(x.PlaintextParentPath)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Prefer the item's original location
                    var destinationFolder = await SafetyHelpers.NoFailureAsync(async () => await AbstractRecycleBinHelpers.GetDestinationFolderAsync(
                        item,
                        _specifics,
                        StreamSerializer.Instance,
                        cancellationToken));

                    if (destinationFolder is null && originalItem is IRecycleBinItem recycleBinItem)
                    {
                        var originalParentPath = Path.GetDirectoryName(recycleBinItem.Id);
                        if (!string.IsNullOrWhiteSpace(originalParentPath))
                        {
                            destinationFolder = await SafetyHelpers.NoFailureAsync(async () =>
                            {
                                var ciphertextItem = await AbstractPathHelpers.GetCiphertextItemAsync(originalParentPath, _specifics, cancellationToken);
                                return ciphertextItem as IModifiableFolder;
                            });
                        }
                    }

                    // Prompt the user to pick the folder when the default destination couldn't be used
                    if (destinationFolder is null)
                    {
                        pickedFolder ??= await GetDestinationFolderAsync();
                        destinationFolder = pickedFolder ?? throw new InvalidOperationException("The destination folder couldn't be chosen.");
                    }

                    // Restore the item to chosen destination
                    await AbstractRecycleBinHelpers.RestoreAsync(
                        item,
                        destinationFolder,
                        _specifics,
                        StreamSerializer.Instance,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // The user canceled the picker (or the operation was canceled) - abort the remaining items
                    throw;
                }
                catch (Exception ex)
                {
                    // A failed item must not abandon the remaining ones
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException("One or more items could not be restored.", exceptions);

            return;

            static int GetPathDepth(string? plaintextPath)
            {
                // Unknown destinations are restored last; the vault root has a depth of zero
                if (plaintextPath is null)
                    return int.MaxValue;

                return plaintextPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Length;
            }

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

                yield return await MaterializeItemAsync(item, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild?> TryGetItemAsync(string ciphertextName, CancellationToken cancellationToken = default)
        {
            if (ciphertextName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || PathHelpers.IsCoreName(ciphertextName))
                return null;

            var item = await _recycleBin.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
            if (item is null)
                return null;

            return await MaterializeItemAsync(item, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (_specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get the associated configuration file. Orphaned payloads (no configuration file)
            // must still be deletable, so a missing configuration is not an error here
            var configurationFile = await _recycleBin.TryGetFileByNameAsync($"{item.Name}.json", cancellationToken);

            // Deserialize configuration
            RecycleBinItemDataModel? itemDataModel = null;
            if (configurationFile is not null)
            {
                itemDataModel = await SafetyHelpers.NoFailureAsync(async () =>
                {
                    await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);
                    return await _serializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);
                });
            }

            // Determine the size to subtract before the payload is gone
            var size = itemDataModel?.Size is { } configuredSize and >= 0L
                ? configuredSize
                : await SafetyHelpers.NoFailureAsync(async () => await AbstractRecycleBinHelpers.GetPlaintextSizeAsync(item, _specifics, cancellationToken));

            // Delete both items
            await _recycleBin.DeleteAsync(item, cancellationToken);
            if (configurationFile is not null)
                await _recycleBin.DeleteAsync(configurationFile, cancellationToken);

            // Check if the item had any size
            if (size <= 0L)
                return;

            // Update occupied size
            await AbstractRecycleBinHelpers.AdjustOccupiedSizeAsync(_recycleBin, _specifics, -size, cancellationToken);
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
        
        private async Task<IRecycleBinItem> MaterializeItemAsync(IStorableChild item, CancellationToken cancellationToken)
        {
            var recycleBinItem = await SafetyHelpers.NoFailureAsync(async () =>
            {
                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, _recycleBin, StreamSerializer.Instance, cancellationToken);
                if (dataModel.ParentId is null || dataModel.Name is null)
                    return null;

                // Decrypt name and parent path
                var plaintextName = dataModel.DecryptName(_specifics.Security) ?? dataModel.Name;
                var plaintextParentId = dataModel.DecryptParentId(_specifics.Security) ?? dataModel.ParentId;

                return new RecycleBinItem(WrapCiphertextItem(item, plaintextName), dataModel, this)
                {
                    Id = string.IsNullOrEmpty(plaintextParentId) || string.IsNullOrEmpty(plaintextName) ? string.Empty : Path.Combine(plaintextParentId, plaintextName),
                    Name = plaintextName ?? item.Name
                };
            });

            // Payloads with a missing or corrupt configuration file are surfaced as unnamed
            // items so they can still be permanently deleted instead of silently occupying space
            return recycleBinItem ?? new RecycleBinItem(WrapCiphertextItem(item, item.Name), OrphanedItemDataModel(), this)
            {
                Id = string.Empty,
                Name = item.Name
            };

            IStorable WrapCiphertextItem(IStorableChild ciphertextItem, string plaintextName)
            {
                return ciphertextItem switch
                {
                    IFile ciphertextFile => new CryptoFile($"/{plaintextName}", ciphertextFile, _specifics),
                    IFolder ciphertextFolder => new CryptoFolder($"/{plaintextName}", ciphertextFolder, _specifics),
                    _ => throw new ArgumentOutOfRangeException(nameof(ciphertextItem))
                };
            }

            static RecycleBinItemDataModel OrphanedItemDataModel()
            {
                return new()
                {
                    Name = null,
                    ParentId = null,
                    DirectoryId = null,
                    DeletionTimestamp = null,
                    Size = -1L
                };
            }
        }
    }
}
