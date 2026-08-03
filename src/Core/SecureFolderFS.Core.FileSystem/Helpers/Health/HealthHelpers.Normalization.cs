using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        /// <summary>
        /// Repairs a Base4K-encoded file name by renaming it from its current (NFD) form to NFC.
        /// </summary>
        /// <param name="affected">The storable item whose name needs normalization.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<IResult> RepairNormalizationAsync(IStorableChild affected, CancellationToken cancellationToken = default)
        {
            try
            {
                var parent = await affected.GetParentAsync(cancellationToken);
                if (parent is not IRenamableFolder renamableFolder)
                    return Result.Failure(FolderNotRenamable);

                var normalizedName = affected.Name.Normalize(NormalizationForm.FormC);
                _ = await renamableFolder.RenameAsync(affected, normalizedName, cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}
