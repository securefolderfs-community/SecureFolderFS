using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static partial class AbstractRecycleBinHelpers
    {
        /// <summary>
        /// The threshold in milliseconds for detecting recently created files.
        /// Files created within this threshold will be deleted immediately instead of recycled.
        /// This helps work around macOS Finder behavior during copy operations.
        /// </summary>
        private const int RECENT_FILE_THRESHOLD_MS = 3000;

        public static async Task<IModifiableFolder?> GetDestinationFolderAsync(IStorableChild recycleBinItem, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is null)
                throw new DirectoryNotFoundException("Could not find recycle bin folder.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(recycleBinItem, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { ParentId: not null, Name: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Get the plaintext name and parent ID
            var plaintextParentPath = deserialized.DecryptParentId(specifics.Security);
            var plaintextOriginalName = deserialized.DecryptName(specifics.Security);
            if (plaintextOriginalName is null || plaintextParentPath is null)
                return null;

            try
            {
                var ciphertextParent = await AbstractPathHelpers.GetCiphertextItemAsync(plaintextParentPath, specifics, cancellationToken);
                return ciphertextParent as IModifiableFolder;
            }
            catch (Exception)
            {
                // Destination folder does not exist, user must choose a new location
                return null;
            }
        }

        public static async Task RestoreAsync(IStorableChild recycleBinItem, IModifiableFolder ciphertextDestinationFolder, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get recycle bin
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not modifiable.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(recycleBinItem, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { ParentId: not null, Name: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Get the plaintext name and parent ID
            var plaintextParentPath = deserialized.DecryptParentId(specifics.Security);
            var plaintextOriginalName = deserialized.DecryptName(specifics.Security);
            if (plaintextOriginalName is null || plaintextParentPath is null)
                throw new FormatException("Could not decrypt recycle bin configuration file.");

            // The name is always re-encrypted for discovery for the destination folder so that the
            // ciphertext name matches the destination's Directory ID, whether the destination is the original parent.
            var ciphertextName = await AbstractPathHelpers.EncryptNameForDiscoveryAsync(plaintextOriginalName, ciphertextDestinationFolder, specifics, cancellationToken);

            // Get an available name if the destination already exists
            var (_, finalPlaintextName) = await GetAvailableDestinationNameAsync(ciphertextDestinationFolder, ciphertextName, plaintextOriginalName, specifics, cancellationToken);

            // Materialize the name: writes the sidecar file if the name is shortened
            ciphertextName = await AbstractPathHelpers.EncryptNameForUseAsync(finalPlaintextName, ciphertextDestinationFolder, specifics, cancellationToken);

            // Rename and move item to destination
            _ = await ciphertextDestinationFolder.MoveStorableFromAsync(recycleBinItem, modifiableRecycleBin, false, ciphertextName, null, cancellationToken);

            // Delete the old configuration file
            var configurationFile = await recycleBin.TryGetFileByNameAsync($"{recycleBinItem.Name}.json", cancellationToken);
            if (configurationFile is not null)
                await modifiableRecycleBin.DeleteAsync(configurationFile, cancellationToken);

            // Check if the item had any size
            if (deserialized.Size is not ({ } size and > 0L))
                return;

            // Update occupied size
            await AdjustOccupiedSizeAsync(modifiableRecycleBin, specifics, -size, cancellationToken);
        }

        public static async Task DeleteOrRecycleAsync(
            IModifiableFolder ciphertextSourceFolder,
            IStorableChild ciphertextItem,
            FileSystemSpecifics specifics,
            IAsyncSerializer<Stream> streamSerializer,
            long sizeHint = -1L,
            bool deleteImmediately = false,
            CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            if (!specifics.Options.IsRecycleBinEnabled() || deleteImmediately)
            {
                await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                return;
            }

            // Allocate Directory ID for later use
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextSourceFolder.Id);

            // Decrypt the plaintext name
            var plaintextName = await AbstractPathHelpers.DecryptNameAsync(ciphertextItem.Name, ciphertextSourceFolder, specifics, directoryId, cancellationToken);
            if (plaintextName is null)
                throw new FormatException("Could not decrypt name for recycle bin configuration file.");

            // Check for wildcard file names
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                if (plaintextName == ".DS_Store" || plaintextName.StartsWith("._", StringComparison.Ordinal))
                {
                    // .DS_Store and Apple Double files are unsupported by the recycle bin, delete immediately
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }

                // Check if the file was recently created (likely part of a copy operation)
                // On macOS, Finder creates files and immediately deletes them during copy operations
                if (await IsRecentlyCreatedAsync(ciphertextItem, cancellationToken))
                {
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }
            }

            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not modifiable.");

            // The size is always calculated (even for unlimited-capacity bins) so that
            // the occupied-size accounting stays accurate if a limit is set later on.
            // Sizes are measured in plaintext bytes on every code path.
            if (sizeHint < 0L)
                sizeHint = await GetPlaintextSizeAsync(ciphertextItem, specifics, cancellationToken);
            sizeHint = Math.Max(0L, sizeHint);

            await specifics.RecycleBinSemaphore.WaitAsync(cancellationToken);
            try
            {
                var occupiedSize = await GetOccupiedSizeAsync(modifiableRecycleBin, cancellationToken);
                if (specifics.Options.RecycleBinSize > 0L && specifics.Options.RecycleBinSize - occupiedSize < sizeHint)
                {
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }

                // The folder's original plaintext path must be captured before the move as it is
                // the key that previously recycled children are folded back in by
                var plaintextFolderPath = ciphertextItem is IFolder
                    ? await SafetyHelpers.NoFailureAsync(async () => await AbstractPathHelpers.GetPlaintextPathAsync(ciphertextItem, specifics, cancellationToken))
                    : null;

                // Decrypt the plaintext parent ID
                var plaintextParentId = await AbstractPathHelpers.GetPlaintextPathAsync((IStorableChild)ciphertextSourceFolder, specifics, cancellationToken);
                if (plaintextParentId is null)
                    throw new FormatException("Could not decrypt parent path for the recycle bin configuration file.");

                // Determine if Directory ID is present
                var isDirectoryIdPresent = ciphertextSourceFolder.Id != specifics.ContentFolder.Id;

                // Encrypt the new plaintext name and parent ID
                var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var dataModel = new RecycleBinItemDataModel()
                {
                    Name = newCiphertextName,
                    ParentId = newCiphertextParentId,
                    DirectoryId = isDirectoryIdPresent ? directoryId : [],
                    DeletionTimestamp = DateTime.Now,
                    Size = sizeHint
                };

                // Create the configuration file before moving the payload. If the move fails,
                // the leftover configuration file is harmless and gets cleaned up on recalculation,
                // whereas a payload without a configuration file would be unrestorable.
                var guid = Guid.NewGuid().ToString();
                var configurationFile = await modifiableRecycleBin.CreateFileAsync($"{guid}.json", false, cancellationToken);
                await WriteItemDataModelAsync(configurationFile, dataModel, streamSerializer, cancellationToken);

                IStorableChild movedItem;
                try
                {
                    // Rename and move item
                    movedItem = await modifiableRecycleBin.MoveStorableFromAsync(ciphertextItem, ciphertextSourceFolder, false, guid, null, cancellationToken);
                }
                catch (Exception)
                {
                    // Best-effort cleanup of the now-orphaned configuration file
                    await SafetyHelpers.NoFailureAsync(async () => await modifiableRecycleBin.DeleteAsync(configurationFile, CancellationToken.None));
                    throw;
                }

                // Reattach previously recycled children of this folder so that trees deleted
                // member-by-member appear as a single restorable entry
                if (plaintextFolderPath is not null && movedItem is IModifiableFolder recycledFolder)
                {
                    var foldedSize = await SafetyHelpers.NoFailureAsync(async () =>
                        await FoldDescendantEntriesAsync(modifiableRecycleBin, recycledFolder, plaintextFolderPath, specifics, streamSerializer, cancellationToken));

                    // The folded sizes were already part of the occupied total; only the
                    // folder's own entry needs to account for its regained contents
                    if (foldedSize > 0L)
                        await WriteItemDataModelAsync(configurationFile, dataModel with { Size = sizeHint + foldedSize }, streamSerializer, cancellationToken);
                }

                // Update occupied size
                await SetOccupiedSizeAsync(modifiableRecycleBin, occupiedSize + sizeHint, cancellationToken);
            }
            finally
            {
                _ = specifics.RecycleBinSemaphore.Release();
            }

            static async Task DeleteImmediatelyAsync(IModifiableFolder ciphertextSourceFolder, IStorableChild ciphertextItem, CancellationToken cancellationToken)
            {
                await ciphertextSourceFolder.DeleteAsync(ciphertextItem, deleteImmediately: true, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Folds previously recycled children of the folder at <paramref name="plaintextFolderPath"/> back into
        /// its recycled payload. OS clients (Finder, Explorer, WebDav/FUSE drivers) delete folder trees
        /// member-by-member, bottom-up; when the parent folder itself arrives at the recycle bin, every entry
        /// that was deleted out of it can be reattached so the tree appears as a single restorable entry.
        /// <br/><br/>
        /// Membership is proven by comparing each entry's stored Directory ID with the recycled folder's
        /// <c>dirid.iv</c> - an exact lineage match that a recreated folder at the same path cannot satisfy.
        /// The original ciphertext names are reproduced exactly because name encryption is deterministic.
        /// </summary>
        /// <remarks>
        /// This method must be invoked while holding <see cref="FileSystemSpecifics.RecycleBinSemaphore"/>.
        /// </remarks>
        /// <returns>The accumulated size in bytes of all folded entries.</returns>
        internal static async Task<long> FoldDescendantEntriesAsync(
            IModifiableFolder recycleBin,
            IModifiableFolder recycledFolder,
            string plaintextFolderPath,
            FileSystemSpecifics specifics,
            IAsyncSerializer<Stream> streamSerializer,
            CancellationToken cancellationToken)
        {
            // Read the recycled folder's Directory ID - children of this exact folder
            // incarnation carry it in their configuration files
            var directoryIdFile = await recycledFolder.TryGetFileByNameAsync(Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken);
            if (directoryIdFile is null)
                return 0L;

            var folderDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            await using (var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken))
            {
                if (await directoryIdStream.ReadAtLeastAsync(folderDirectoryId, Constants.DIRECTORY_ID_SIZE, false, cancellationToken) < Constants.DIRECTORY_ID_SIZE)
                    return 0L;
            }

            // Snapshot the configuration files first - folding mutates the recycle bin contents
            var configurationFiles = new List<IChildFile>();
            await foreach (var item in recycleBin.GetItemsAsync(StorableType.File, cancellationToken))
            {
                if (item is IChildFile file && file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    configurationFiles.Add(file);
            }

            var foldedSize = 0L;
            var normalizedFolderPath = Path.TrimEndingDirectorySeparator(plaintextFolderPath);
            foreach (var configurationFile in configurationFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // A single unreadable entry must not abandon the remaining ones
                await SafetyHelpers.NoFailureAsync(async () =>
                {
                    var dataModel = await GetItemDataModelAsync(configurationFile, recycleBin, streamSerializer, cancellationToken);
                    if (dataModel is not { Name: not null, ParentId: not null, DirectoryId: { Length: Constants.DIRECTORY_ID_SIZE } childDirectoryId })
                        return;

                    // Lineage check: the entry must have been deleted out of this exact folder incarnation
                    if (!childDirectoryId.AsSpan().SequenceEqual(folderDirectoryId))
                        return;

                    // Recency check: don't silently pull in unrelated deletions from long ago
                    if (dataModel.DeletionTimestamp is not { } deletionTimestamp
                        || Math.Abs((DateTime.Now - deletionTimestamp).TotalMilliseconds) > Constants.RECYCLE_BIN_FOLD_WINDOW_MS)
                        return;

                    // Sanity check: the original parent path must match the folder being recycled
                    var plaintextParentPath = dataModel.DecryptParentId(specifics.Security);
                    if (plaintextParentPath is null || Path.TrimEndingDirectorySeparator(plaintextParentPath) != normalizedFolderPath)
                        return;

                    var childPlaintextName = dataModel.DecryptName(specifics.Security);
                    if (childPlaintextName is null)
                        return;

                    // Get the payload; configurations without one are cleaned up on recalculation
                    var payloadName = Path.GetFileNameWithoutExtension(configurationFile.Name);
                    if (await recycleBin.TryGetFirstByNameAsync(payloadName, cancellationToken) is not IStorableChild payload)
                        return;

                    // Reconstruct the child's original ciphertext name (deterministic for the folder's Directory ID).
                    // On a name collision the entry is left standalone instead of being overwritten
                    var ciphertextChildName = AbstractPathHelpers.EncryptNewName(childPlaintextName, folderDirectoryId, specifics.Security);
                    if (await recycledFolder.TryGetFirstByNameAsync(ciphertextChildName, cancellationToken) is not null)
                        return;

                    // Reattach the payload and discard the now-redundant configuration file
                    _ = await recycledFolder.MoveStorableFromAsync(payload, recycleBin, false, ciphertextChildName, null, cancellationToken);
                    await recycleBin.DeleteAsync(configurationFile, cancellationToken);

                    if (dataModel.Size is { } size and > 0L)
                        foldedSize += size;
                });
            }

            return foldedSize;
        }

        private static async Task<(string CiphertextName, string PlaintextName)> GetAvailableDestinationNameAsync(IFolder ciphertextDestinationFolder, string ciphertextName, string plaintextOriginalName, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            var finalPlaintextName = plaintextOriginalName;

            // Check if the item already exists
            var existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
            if (existing is not null)
            {
                // If the item already exists, append a suffix to the name
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(plaintextOriginalName);
                var extension = Path.GetExtension(plaintextOriginalName);
                var suffix = 1;
                do
                {
                    finalPlaintextName = $"{nameWithoutExtension} ({suffix}){extension}";
                    ciphertextName = await AbstractPathHelpers.EncryptNameForDiscoveryAsync(finalPlaintextName, ciphertextDestinationFolder, specifics, cancellationToken);
                    existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
                    suffix++;
                } while (existing is not null);
            }

            return (ciphertextName, finalPlaintextName);
        }

        private static async Task<bool> IsRecentlyCreatedAsync(IStorable storable, CancellationToken cancellationToken)
        {
            try
            {
                var dateCreated = await storable.GetDateCreatedAsync(cancellationToken);
                if (dateCreated is null)
                    return false;

                var dateCreatedUtc = dateCreated.Value.ToUniversalTime();
                var elapsedMilliseconds = (DateTime.UtcNow - dateCreatedUtc).TotalMilliseconds;

                // Negative elapsed time indicates clock skew.
                // Never treat such files as recently created, as that would permanently delete them
                return elapsedMilliseconds is >= 0d and <= RECENT_FILE_THRESHOLD_MS;
            }
            catch
            {
                // If we can't determine creation time, assume it's not recent
                return false;
            }
        }
    }
}
