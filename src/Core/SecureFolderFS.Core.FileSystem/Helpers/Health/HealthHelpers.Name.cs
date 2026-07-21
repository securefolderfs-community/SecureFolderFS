using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        public static async Task<IResult> RepairNameAsync(IStorableChild affected, FileSystemSpecifics specifics, string newName, CancellationToken cancellationToken)
        {
            // Return success if no encryption is used
            if (specifics.Security.NameCrypt is null)
                return Result.Success;

            try
            {
                // Get parent folder
                var parentFolder = await affected.GetParentAsync(cancellationToken);
                if (parentFolder is not IRenamableFolder renamableFolder)
                    return Result.Failure(FolderNotRenamable);

                // Remember old name for sidecar cleanup
                var oldName = affected.Name;

                // Encrypt new name (writes sidecar if shortening applies) and rename
                var encryptedName = await AbstractPathHelpers.EncryptNameForUseAsync(newName, parentFolder, specifics, cancellationToken);
                _ = await renamableFolder.RenameAsync(affected, encryptedName, cancellationToken);

                // Clean up old sidecar if the previous name was shortened
                await AbstractPathHelpers.DeleteSidecarFileAsync(oldName, renamableFolder, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }

            // Update cache
            if (specifics.CiphertextFileNameCache.IsAvailable)
            {
                await SafetyHelpers.NoFailureAsync(async () =>
                {
                    var parent = await affected.GetParentAsync(cancellationToken);
                    if (parent is null)
                        return;

                    var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);
                    var isAllocated = await AbstractPathHelpers.GetDirectoryIdAsync(parent, specifics, directoryId, cancellationToken);
                    specifics.CiphertextFileNameCache.CacheRemove(new(isAllocated ? directoryId : [], affected.Name));
                });
            }

            return Result.Success;
        }
    }
}
