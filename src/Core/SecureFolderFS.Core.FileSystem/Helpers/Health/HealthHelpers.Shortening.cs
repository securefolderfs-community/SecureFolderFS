using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        /// <summary>
        /// Detects orphan sidecar files in the specified folder and reports them as issues.
        /// An orphan sidecar is a <c>.sffsi</c> file with no matching <c>.sffsn</c> companion.
        /// </summary>
        /// <param name="folder">The folder to scan for orphan sidecars.</param>
        /// <param name="reporter">The progress reporter for reporting detected issues.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task DetectOrphanSidecarsAsync(IFolder folder, IProgress<IResult>? reporter, CancellationToken cancellationToken = default)
        {
            if (reporter is null)
                return;

            var shortenedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sidecars = new List<IStorableChild>();

            await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (AbstractPathHelpers.IsSidecarName(item.Name))
                    sidecars.Add(item);
                else if (item.Name.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                    shortenedNames.Add(item.Name);
            }

            foreach (var sidecar in sidecars)
            {
                // Derive the expected companion (replace .sffsi with .sffsn)
                var baseName = sidecar.Name[..^Constants.Names.SIDECAR_FILE_EXTENSION.Length];
                var expectedCompanion = baseName + Constants.Names.SHORTENED_FILE_EXTENSION;

                if (!shortenedNames.Contains(expectedCompanion))
                    reporter.Report(Result<IStorable>.Failure(sidecar, new OrphanSidecarException(sidecar.Name)));
            }
        }

        /// <summary>
        /// Deletes an orphan sidecar file.
        /// </summary>
        /// <param name="sidecar">The orphan sidecar file to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<IResult> DeleteOrphanSidecarAsync(IStorableChild sidecar, CancellationToken cancellationToken = default)
        {
            try
            {
                if (sidecar is not IChildFile childFile)
                    return Result.Failure(new InvalidOperationException("Sidecar is not a child file."));

                var parent = await childFile.GetParentAsync(cancellationToken);
                if (parent is not IModifiableFolder modifiableFolder)
                    return Result.Failure(new InvalidOperationException("Parent folder does not support deletion."));

                await modifiableFolder.DeleteAsync(childFile, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}
