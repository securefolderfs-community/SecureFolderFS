using System;
using System.IO;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native
{
    public static partial class NativeRecycleBinHelpers
    {
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
                if (plaintextName == ".DS_Store" || (plaintextName?.StartsWith("._", StringComparison.Ordinal) ?? false))
                {
                    // .DS_Store and Apple Double files are unsupported by the recycle bin, delete immediately
                    DeleteImmediately(ciphertextPath, storableType);
                    return;
                }
            }

            var recycleBinPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME);
            _ = Directory.CreateDirectory(recycleBinPath);

            if (sizeHint < 0L && specifics.Options.RecycleBinSize > 0L)
            {
                sizeHint = storableType switch
                {
                    StorableType.File => new FileInfo(ciphertextPath).Length,
                    StorableType.Folder => GetFolderSizeRecursive(ciphertextPath),
                    _ => 0L
                };

                var occupiedSize = GetOccupiedSize(specifics);
                var availableSize = specifics.Options.RecycleBinSize - occupiedSize;
                if (availableSize < sizeHint)
                {
                    DeleteImmediately(ciphertextPath, storableType);
                    return;
                }
            }

            // Move and rename item
            var guid = Guid.NewGuid().ToString();
            var destinationPath = Path.Combine(recycleBinPath, guid);
            if (storableType == StorableType.Folder)
                Directory.Move(ciphertextPath, destinationPath);
            else
                File.Move(ciphertextPath, destinationPath);

            // Create the configuration file
            using (var configurationStream = File.Create($"{destinationPath}.json"))
            {
                // Decrypt the plaintext parent ID
                var plaintextParentId = NativePathHelpers.GetPlaintextPath(ciphertextParentPath, specifics);
                if (plaintextParentId is null || plaintextName is null)
                    throw new FormatException("Could not decrypt parent path for recycle bin configuration file.");

                // Determine if Directory ID is present
                var isDirectoryIdPresent = ciphertextParentPath != Path.DirectorySeparatorChar.ToString() && ciphertextParentPath != specifics.ContentFolder.Id;

                // Encrypt the new plaintext name and parent ID
                var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);

                // Serialize configuration data model
                using var serializedStream = StreamSerializer.Instance.SerializeAsync(
                    new RecycleBinItemDataModel()
                    {
                        Name = newCiphertextName,
                        ParentId = newCiphertextParentId,
                        DirectoryId = isDirectoryIdPresent ? directoryId : [],
                        DeletionTimestamp = DateTime.Now,
                        Size = sizeHint
                    }).ConfigureAwait(false).GetAwaiter().GetResult();

                // Write to destination stream
                serializedStream.CopyTo(configurationStream);
                serializedStream.Flush();
            }

            // Update occupied size
            if (specifics.Options.IsRecycleBinEnabled())
            {
                var occupiedSize = GetOccupiedSize(specifics);
                var newSize = occupiedSize + sizeHint;
                SetOccupiedSize(specifics, newSize);
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
        }
    }
}
