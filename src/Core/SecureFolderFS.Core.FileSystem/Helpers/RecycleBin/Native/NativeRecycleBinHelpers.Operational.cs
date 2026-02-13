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
                var plaintextPath = NativePathHelpers.GetPlaintextPath(ciphertextPath, specifics);
                var plaintextName = Path.GetFileName(plaintextPath) ?? string.Empty;
                if (plaintextName == ".DS_Store" || plaintextName.StartsWith("._", StringComparison.Ordinal))
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
            var directoryIdResult = NativePathHelpers.GetDirectoryId(ciphertextPath, specifics, directoryId);

            // Move and rename item
            var guid = Guid.NewGuid().ToString();
            var destinationPath = Path.Combine(recycleBinPath, guid);
            Directory.Move(ciphertextPath, destinationPath);

            // Create configuration file
            using (var configurationStream = File.Create($"{destinationPath}.json"))
            {
                // Serialize configuration data model
                using var serializedStream = StreamSerializer.Instance.SerializeAsync(
                    new RecycleBinItemDataModel()
                    {
                        OriginalName = Path.GetFileName(ciphertextPath),
                        ParentPath = Path.GetDirectoryName(ciphertextPath)?.Replace(specifics.ContentFolder.Id, string.Empty).Replace(Path.DirectorySeparatorChar, '/') ?? string.Empty,
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
                if (type == StorableType.None)
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
