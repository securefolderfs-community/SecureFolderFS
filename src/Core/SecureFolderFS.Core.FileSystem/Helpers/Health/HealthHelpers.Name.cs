using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
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
            var repairResult = await RepairNameAsync(affected, specifics.Security, specifics.ContentFolder, newName, cancellationToken);
            if (!repairResult.Successful || !specifics.CiphertextFileNameCache.IsAvailable)
                return repairResult;

            // Update cache
            await SafetyHelpers.NoFailureAsync(async () =>
            {
                var parent = await affected.GetParentAsync(cancellationToken);
                if (parent is null)
                    return;

                var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);
                var isAllocated = await AbstractPathHelpers.GetDirectoryIdAsync(parent, specifics, directoryId, cancellationToken);
                specifics.CiphertextFileNameCache.CacheRemove(new(isAllocated ? directoryId : [], affected.Name));
            });

            return repairResult;
        }

        private static async Task<IResult> RepairNameAsync(IStorableChild affected, Security security, IFolder contentFolder, string newName, CancellationToken cancellationToken)
        {
            // Return success if no encryption is used
            if (security.NameCrypt is null)
                return Result.Success;

            try
            {
                // Get Directory ID
                var parentFolder = await affected.GetParentAsync(cancellationToken);
                if (parentFolder is not IRenamableFolder renamableFolder)
                    return Result.Failure(FolderNotRenamable);

                // Encrypt new name and rename
                var encryptedName = await AbstractPathHelpers.EncryptNameAsync(newName, parentFolder, contentFolder, security, cancellationToken);
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
