using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
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
            var plaintextOriginalName = deserialized.DecryptName(specifics.Security);
            if (plaintextOriginalName is null)
                throw new FormatException("Could not decrypt recycle bin configuration file.");

            // The name is always re-encrypted for the destination folder so that the
            // ciphertext name matches the destination's Directory ID, whether the destination is the original parent.
            var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(plaintextOriginalName, ciphertextDestinationFolder, specifics, cancellationToken);

            // Get an available name if the destination already exists
            ciphertextName = await GetAvailableDestinationNameAsync(ciphertextDestinationFolder, ciphertextName, plaintextOriginalName, specifics, cancellationToken);

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

                // Create the configuration file before moving the payload. If the move fails,
                // the leftover configuration file is harmless and gets cleaned up on recalculation,
                // whereas a payload without a configuration file would be unrestorable.
                var guid = Guid.NewGuid().ToString();
                var configurationFile = await modifiableRecycleBin.CreateFileAsync($"{guid}.json", false, cancellationToken);
                await using (var configurationStream = await configurationFile.OpenWriteAsync(cancellationToken))
                {
                    // Decrypt the plaintext parent ID
                    var plaintextParentId = await AbstractPathHelpers.GetPlaintextPathAsync((IStorableChild)ciphertextSourceFolder, specifics, cancellationToken);
                    if (plaintextParentId is null)
                        throw new FormatException("Could not decrypt parent path for the recycle bin configuration file.");

                    // Determine if Directory ID is present
                    var isDirectoryIdPresent = ciphertextSourceFolder.Id != specifics.ContentFolder.Id;

                    // Encrypt the new plaintext name and parent ID
                    var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                    var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);

                    // Serialize configuration data model
                    await using var serializedStream = await streamSerializer.SerializeAsync(
                        new RecycleBinItemDataModel()
                        {
                            Name = newCiphertextName,
                            ParentId = newCiphertextParentId,
                            DirectoryId = isDirectoryIdPresent ? directoryId : [],
                            DeletionTimestamp = DateTime.Now,
                            Size = sizeHint
                        }, cancellationToken);

                    // Write to destination stream
                    await serializedStream.CopyToAsync(configurationStream, cancellationToken);
                    await configurationStream.FlushAsync(cancellationToken);
                }

                try
                {
                    // Rename and move item
                    _ = await modifiableRecycleBin.MoveStorableFromAsync(ciphertextItem, ciphertextSourceFolder, false, guid, null, cancellationToken);
                }
                catch (Exception)
                {
                    // Best-effort cleanup of the now-orphaned configuration file
                    await SafetyHelpers.NoFailureAsync(async () => await modifiableRecycleBin.DeleteAsync(configurationFile, CancellationToken.None));
                    throw;
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

        private static async Task<string> GetAvailableDestinationNameAsync(IFolder ciphertextDestinationFolder, string ciphertextName, string plaintextOriginalName, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            // Check if the item already exists
            var existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
            if (existing is null)
                return ciphertextName;

            // If the item already exists, append a suffix to the name
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(plaintextOriginalName);
            var extension = Path.GetExtension(plaintextOriginalName);
            var suffix = 1;
            do
            {
                var newPlaintextName = $"{nameWithoutExtension} ({suffix}){extension}";
                ciphertextName = await AbstractPathHelpers.EncryptNameAsync(newPlaintextName, ciphertextDestinationFolder, specifics, cancellationToken);
                existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
                suffix++;
            } while (existing is not null);

            return ciphertextName;
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
