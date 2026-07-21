using System;
using System.IO;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native
{
    public static partial class NativeRecycleBinHelpers
    {
        /// <summary>
        /// The threshold in milliseconds for detecting recently created files.
        /// Files created within this threshold will be deleted immediately instead of recycled.
        /// This helps work around macOS Finder behavior during copy operations.
        /// </summary>
        private const int RECENT_FILE_THRESHOLD_MS = 3000;

        public static void DeleteOrRecycle(string ciphertextPath, FileSystemSpecifics specifics, StorableType storableType, long sizeHint = -1L)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            storableType = AlignStorableType(ciphertextPath);

            // Compute parent path early so it's available in all branches
            var ciphertextParentPath = Path.GetDirectoryName(ciphertextPath);
            _ = ciphertextParentPath ?? throw new DirectoryNotFoundException("The parent folder could not be determined.");

            if (!specifics.Options.IsRecycleBinEnabled())
            {
                DeleteImmediately(ciphertextPath, storableType);
                return;
            }

            // Allocate Directory ID for later use
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);

            // Decrypt the plaintext name (reads sidecar if shortened)
            var plaintextName = NativePathHelpers.DecryptName(Path.GetFileName(ciphertextPath), ciphertextParentPath, specifics, directoryId);
            if (plaintextName is null)
                throw new FormatException("Could not decrypt name for recycle bin configuration file.");

            // Check for wildcard file names
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                if (plaintextName == ".DS_Store" || plaintextName.StartsWith("._", StringComparison.Ordinal))
                {
                    // .DS_Store and Apple Double files are unsupported by the recycle bin, delete immediately
                    DeleteImmediately(ciphertextPath, storableType);
                    return;
                }

                // Check if the file was recently created (likely part of a copy operation)
                // On macOS, Finder creates files and immediately deletes them during copy operations
                if (storableType == StorableType.File && IsRecentlyCreated(ciphertextPath))
                {
                    DeleteImmediately(ciphertextPath, storableType);
                    return;
                }
            }

            var recycleBinPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME);
            _ = Directory.CreateDirectory(recycleBinPath);

            // The size is always calculated (even for unlimited-capacity bins) so that
            // the occupied-size accounting stays accurate if a limit is set later on.
            // Sizes are measured in plaintext bytes on every code path.
            if (sizeHint < 0L)
            {
                sizeHint = storableType switch
                {
                    StorableType.File => CalculatePlaintextSize(new FileInfo(ciphertextPath).Length, specifics),
                    StorableType.Folder => GetFolderPlaintextSize(ciphertextPath, specifics),
                    _ => 0L
                };
            }
            sizeHint = Math.Max(0L, sizeHint);

            specifics.RecycleBinSemaphore.Wait();
            try
            {
                var occupiedSize = GetOccupiedSize(specifics);
                if (specifics.Options.RecycleBinSize > 0L && specifics.Options.RecycleBinSize - occupiedSize < sizeHint)
                {
                    DeleteImmediately(ciphertextPath, storableType);
                    return;
                }

                // Decrypt the plaintext parent ID
                var plaintextParentId = NativePathHelpers.GetPlaintextPath(ciphertextParentPath, specifics);
                if (plaintextParentId is null)
                    throw new FormatException("Could not decrypt parent path for recycle bin configuration file.");

                // Determine if Directory ID is present (i.e., the source folder is not the content root)
                var normalizedParentPath = Path.TrimEndingDirectorySeparator(ciphertextParentPath);
                var isDirectoryIdPresent = normalizedParentPath != Path.DirectorySeparatorChar.ToString()
                                           && normalizedParentPath != Path.TrimEndingDirectorySeparator(specifics.ContentFolder.Id);

                // Encrypt the new plaintext name and parent ID
                var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var dataModel = new RecycleBinItemDataModel()
                {
                    Name = newCiphertextName,
                    ParentId = newCiphertextParentId,
                    DirectoryId = isDirectoryIdPresent ? directoryId : Array.Empty<byte>(),
                    DeletionTimestamp = DateTime.Now,
                    Size = sizeHint
                };

                // Create the configuration file before moving the payload. If the move fails,
                // the leftover configuration file is harmless and gets cleaned up on recalculation,
                // whereas a payload without a configuration file would be unrestorable.
                var guid = Guid.NewGuid().ToString();
                var destinationPath = Path.Combine(recycleBinPath, guid);
                var configurationPath = $"{destinationPath}.json";
                WriteItemDataModel(configurationPath, dataModel);

                // The folder's original plaintext path must be captured before the move - it is
                // the key that previously recycled children are folded back in by
                var plaintextFolderPath = storableType == StorableType.Folder
                    ? SafetyHelpers.NoFailureResult(() => NativePathHelpers.GetPlaintextPath(ciphertextPath, specifics))
                    : null;

                try
                {
                    // Move and rename item
                    if (storableType == StorableType.Folder)
                        Directory.Move(ciphertextPath, destinationPath);
                    else
                        File.Move(ciphertextPath, destinationPath);
                }
                catch (Exception)
                {
                    // Best-effort cleanup of the now-orphaned configuration file
                    SafetyHelpers.NoFailure(() => File.Delete(configurationPath));
                    throw;
                }

                // Reattach previously recycled children of this folder so that trees deleted
                // member-by-member appear as a single restorable entry
                if (plaintextFolderPath is not null)
                {
                    var foldedSize = SafetyHelpers.NoFailureResult(() => FoldDescendantEntries(recycleBinPath, destinationPath, plaintextFolderPath, specifics));
                    if (foldedSize > 0L)
                    {
                        // The folded sizes were already part of the occupied total; only the
                        // folder's own entry needs to account for its regained contents
                        SafetyHelpers.NoFailure(() => WriteItemDataModel(configurationPath, dataModel with { Size = sizeHint + foldedSize }));
                    }
                }

                // Update occupied size
                SetOccupiedSize(specifics, occupiedSize + sizeHint);
            }
            finally
            {
                _ = specifics.RecycleBinSemaphore.Release();
            }

            return;

            StorableType AlignStorableType(string path)
            {
                var type = storableType is StorableType.File or StorableType.Folder ? storableType : GetStorableType(path);
                if (type is not (StorableType.File or StorableType.Folder))
                    throw new FileNotFoundException("The item could not be determined.");

                return type;
            }

            static StorableType GetStorableType(string path)
            {
                if (File.Exists(path))
                    return StorableType.File;

                if (Directory.Exists(path))
                    return StorableType.Folder;

                return StorableType.None;
            }

            static void DeleteImmediately(string path, StorableType type)
            {
                if (type == StorableType.File)
                    File.Delete(path);
                else if (type == StorableType.Folder)
                    Directory.Delete(path, true);
            }

            static bool IsRecentlyCreated(string path)
            {
                try
                {
                    var elapsedMilliseconds = (DateTime.UtcNow - File.GetCreationTimeUtc(path)).TotalMilliseconds;

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

        /// <summary>
        /// Serializes <paramref name="dataModel"/> into the file at <paramref name="configurationPath"/>, truncating any previous content.
        /// </summary>
        private static void WriteItemDataModel(string configurationPath, RecycleBinItemDataModel dataModel)
        {
            using var configurationStream = File.Create(configurationPath);
            using var serializedStream = StreamSerializer.Instance.SerializeAsync(dataModel).ConfigureAwait(false).GetAwaiter().GetResult();

            serializedStream.CopyTo(configurationStream);
            configurationStream.Flush();
        }

        /// <summary>
        /// Folds previously recycled children of the folder at <paramref name="plaintextFolderPath"/> back into
        /// its recycled payload at <paramref name="recycledFolderPath"/>. OS clients (Finder, Explorer, WebDav/FUSE
        /// drivers) delete folder trees member-by-member, bottom-up; when the parent folder itself arrives at the
        /// recycle bin, every entry that was deleted out of it can be reattached so the tree appears as a single
        /// restorable entry.
        /// <br/><br/>
        /// Membership is proven by comparing each entry's stored Directory ID with the recycled folder's
        /// <c>dirid.iv</c> - an exact lineage match that a recreated folder at the same path cannot satisfy.
        /// The original ciphertext names are reproduced exactly because name encryption is deterministic.
        /// </summary>
        /// <remarks>
        /// This method must be invoked while holding <see cref="FileSystemSpecifics.RecycleBinSemaphore"/>.
        /// </remarks>
        /// <returns>The accumulated size in bytes of all folded entries.</returns>
        private static long FoldDescendantEntries(string recycleBinPath, string recycledFolderPath, string plaintextFolderPath, FileSystemSpecifics specifics)
        {
            // Read the recycled folder's Directory ID - children of this exact folder
            // incarnation carry it in their configuration files
            var directoryIdPath = Path.Combine(recycledFolderPath, Constants.Names.DIRECTORY_ID_FILENAME);
            if (!File.Exists(directoryIdPath))
                return 0L;

            var folderDirectoryId = File.ReadAllBytes(directoryIdPath);
            if (folderDirectoryId.Length != Constants.DIRECTORY_ID_SIZE)
                return 0L;

            var foldedSize = 0L;
            var normalizedFolderPath = Path.TrimEndingDirectorySeparator(plaintextFolderPath);

            // Snapshot the configuration files first - folding mutates the recycle bin contents
            foreach (var configurationPath in Directory.GetFiles(recycleBinPath, "*.json"))
            {
                // A single unreadable entry must not abandon the remaining ones
                SafetyHelpers.NoFailure(() =>
                {
                    RecycleBinItemDataModel? dataModel;
                    using (var configurationStream = File.Open(configurationPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        dataModel = StreamSerializer.Instance.TryDeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream).ConfigureAwait(false).GetAwaiter().GetResult();

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

                    // Reconstruct the child's original ciphertext name (deterministic for the folder's Directory ID).
                    // On a name collision the entry is left standalone instead of being overwritten
                    var ciphertextChildName = AbstractPathHelpers.EncryptNewName(childPlaintextName, folderDirectoryId, specifics.Security);
                    var reattachedPath = Path.Combine(recycledFolderPath, ciphertextChildName);
                    if (Path.Exists(reattachedPath))
                        return;

                    // Reattach the payload; configurations without one are cleaned up on recalculation
                    var payloadPath = Path.Combine(recycleBinPath, Path.GetFileNameWithoutExtension(configurationPath));
                    if (Directory.Exists(payloadPath))
                        Directory.Move(payloadPath, reattachedPath);
                    else if (File.Exists(payloadPath))
                        File.Move(payloadPath, reattachedPath);
                    else
                        return;

                    // Discard the now-redundant configuration file
                    File.Delete(configurationPath);

                    if (dataModel.Size is { } size and > 0L)
                        foldedSize += size;
                });
            }

            return foldedSize;
        }
    }
}
