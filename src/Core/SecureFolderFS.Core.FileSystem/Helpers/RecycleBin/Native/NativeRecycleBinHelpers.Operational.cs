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
            if (!specifics.Options.IsRecycleBinEnabled())
            {
                DeleteImmediately(ciphertextPath, storableType);
                return;
            }

            // Allocate Directory ID for later use
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);

            // Decrypt the plaintext name
            var ciphertextParentPath = Path.GetDirectoryName(ciphertextPath);
            _ = ciphertextParentPath ?? throw new DirectoryNotFoundException("The parent folder could not be determined.");
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

                // Create the configuration file before moving the payload. If the move fails,
                // the leftover configuration file is harmless and gets cleaned up on recalculation,
                // whereas a payload without a configuration file would be unrestorable.
                var guid = Guid.NewGuid().ToString();
                var destinationPath = Path.Combine(recycleBinPath, guid);
                var configurationPath = $"{destinationPath}.json";
                using (var configurationStream = File.Create(configurationPath))
                {
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

                    // Serialize configuration data model
                    using var serializedStream = StreamSerializer.Instance.SerializeAsync(
                        new RecycleBinItemDataModel()
                        {
                            Name = newCiphertextName,
                            ParentId = newCiphertextParentId,
                            DirectoryId = isDirectoryIdPresent ? directoryId : Array.Empty<byte>(),
                            DeletionTimestamp = DateTime.Now,
                            Size = sizeHint
                        }).ConfigureAwait(false).GetAwaiter().GetResult();

                    // Write to destination stream
                    serializedStream.CopyTo(configurationStream);
                    configurationStream.Flush();
                }

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
    }
}
