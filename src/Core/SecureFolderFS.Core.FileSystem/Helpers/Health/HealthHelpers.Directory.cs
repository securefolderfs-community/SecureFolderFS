using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        public static async Task<IResult> RepairDirectoryAsync(IFolder affected, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            // Return success if no encryption is used
            if (specifics.Security.NameCrypt is null)
                return Result.Success;

            if (affected is not IRenamableFolder renamableFolder)
                return Result.Failure(FolderNotRenamable);

            try
            {
                // Overwrite existing DirectoryID
                var directoryIdFile = await renamableFolder.CreateFileAsync(Constants.Names.DIRECTORY_ID_FILENAME, true, cancellationToken);
                await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Write, FileShare.Read, cancellationToken);

                var directoryId = Guid.NewGuid().ToByteArray();
                await directoryIdStream.WriteAsync(directoryId, cancellationToken);

                // Encrypt child item names
                await foreach (var item in affected.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    if (PathHelpers.IsCoreName(item.Name))
                        continue;

                    // Remember old name for sidecar cleanup
                    var oldName = item.Name;

                    // Encrypt a new name (writes sidecar if shortening applies) and rename
                    var encryptedName = await AbstractPathHelpers.EncryptNewNameForUseAsync(item.Name, directoryId, affected, specifics, cancellationToken);
                    _ = await renamableFolder.RenameAsync(item, encryptedName, cancellationToken);

                    // Clean up old sidecar if the previous name was shortened
                    await AbstractPathHelpers.DeleteSidecarFileAsync(oldName, renamableFolder, cancellationToken);
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
