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

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        public static async Task<IResult> RepairNameAsync(IStorableChild affected, FileSystemSpecifics specifics, string newName, CancellationToken cancellationToken)
        {
            return await RepairNameAsync(affected, specifics.Security, specifics.ContentFolder, newName, cancellationToken);

            // TODO: Update caches in FileSystemSpecifics
        }

        public static async Task<IResult> RepairNameAsync(IStorableChild affected, Security security, IFolder contentFolder, string newName, CancellationToken cancellationToken)
        {
            // Return success, if no encryption is used
            if (security.NameCrypt is null)
                return Result.Success;

            try
            {
                // Get Directory ID
                var parentFolder = await affected.GetParentAsync(cancellationToken);
                if (parentFolder is not IRenamableFolder renamableFolder)
                    return Result.Failure(FolderNotRenamable);

                byte[] directoryId;
                if (parentFolder.Id != contentFolder.Id) // TODO: Remove code duplication with AbstractPathHelpers
                {
                    var directoryIdFile = await parentFolder.GetFileByNameAsync(Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken);
                    await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);

                    directoryId = new byte[Constants.DIRECTORY_ID_SIZE];
                    var read = await directoryIdStream.ReadAsync(directoryId, cancellationToken);

                    if (read < Constants.DIRECTORY_ID_SIZE)
                        throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");
                }
                else
                    directoryId = Array.Empty<byte>();

                // Encrypt new name
                var encryptedName = security.NameCrypt.EncryptName(newName, directoryId);
                encryptedName = $"{encryptedName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}";

                // Rename
                _ = await renamableFolder.RenameAsync(affected, encryptedName, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }

            return Result.Success;
        }
    }
}
