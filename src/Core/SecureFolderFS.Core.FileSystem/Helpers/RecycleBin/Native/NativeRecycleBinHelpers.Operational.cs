using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;

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

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                var ciphertextParentPath = Path.GetDirectoryName(ciphertextPath);
                var plaintextName = NativePathHelpers.DecryptName(Path.GetFileName(ciphertextPath), ciphertextParentPath ?? string.Empty, specifics);
                if (plaintextName == ".DS_Store" || (plaintextName?.StartsWith("._", StringComparison.Ordinal) ?? false))
                {
                    // .DS_Store and Apple Double files are not supported by the recycle bin, delete immediately
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

            // Get source Directory ID
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);
            var directoryIdResult = NativePathHelpers.GetDirectoryIdOfChild(ciphertextPath, specifics, directoryId);

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
                var parentCiphertextPath = Path.GetDirectoryName(ciphertextPath);
                if (parentCiphertextPath is null)
                    throw new FileNotFoundException("The parent folder could not be determined.");

                // Decrypt the plaintext name
                var plaintextName = specifics.Security.NameCrypt is not null
                    ? specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(ciphertextPath), directoryIdResult ? directoryId : ReadOnlySpan<byte>.Empty)
                    : Path.GetFileName(ciphertextPath);

                // Decrypt the plaintext parent ID
                var plaintextParentId = NativePathHelpers.GetPlaintextPath(parentCiphertextPath, specifics);
                if (plaintextParentId is null || plaintextName is null)
                    throw new FormatException("Could not decrypt paths for recycle bin configuration file.");

                // Encrypt the new plaintext name and parent ID
                var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, directoryIdResult ? directoryId : []);
                var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, directoryIdResult ? directoryId : []);

                // Serialize configuration data model
                using var serializedStream = StreamSerializer.Instance.SerializeAsync(
                    new RecycleBinItemDataModel()
                    {
                        Name = newCiphertextName,
                        ParentId = newCiphertextParentId,
                        DirectoryId = directoryIdResult ? directoryId : [],
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
