using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        public static async Task<IResult> RepairDirectoryAsync(IFolder affected, Security security, CancellationToken cancellationToken)
        {
            // Return success, if no encryption is used
            if (security.NameCrypt is null)
                return Result.Success;

            if (affected is not IRenamableFolder renamableFolder)
                return Result.Failure(FolderNotRenamable);

            try
            {
                // Overwrite existing DirectoryID
                var directoryIdFile = await renamableFolder.CreateFileAsync(Constants.Names.DIRECTORY_ID_FILENAME, true, cancellationToken);
                await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.ReadWrite, FileShare.Read, cancellationToken);

                var directoryId = Guid.NewGuid().ToByteArray();
                await directoryIdStream.WriteAsync(directoryId, cancellationToken);

                // Encrypt child item names
                await foreach (var item in affected.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    if (PathHelpers.IsCoreName(item.Name))
                        continue;

                    // Encrypt new name
                    var encryptedName = security.NameCrypt.EncryptName(item.Name, directoryId);
                    encryptedName = $"{encryptedName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}";

                    // Rename
                    _ = await renamableFolder.RenameAsync(item, encryptedName, cancellationToken);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}
