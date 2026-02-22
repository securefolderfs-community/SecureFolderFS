using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Health;
using SecureFolderFS.Core.FileSystem.Validators;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultHealthService"/>
    public class VaultHealthService : IVaultHealthService
    {
        /// <inheritdoc/>
        public async Task<HealthIssueViewModel?> GetIssueViewModelAsync(IResult result, IStorableChild storable, CancellationToken cancellation)
        {
            await Task.CompletedTask;
            if (result.Successful || result.Exception is null)
                return null;

            return result.Exception switch
            {
                AggregateException aggregate => aggregate.InnerException switch
                {
                    EndOfStreamException => new HealthDirectoryIssueViewModel(storable, result, "InvalidDirectoryId".ToLocalized()) { ErrorMessage = "RegenerateInvalidDirectoryId".ToLocalized() }.WithInitAsync(),
                    FileNotFoundException => new HealthDirectoryIssueViewModel(storable, result, "MissingDirectoryId".ToLocalized()) { ErrorMessage = "RegenerateMissingDirectoryId".ToLocalized() }.WithInitAsync(),
                    { } ex => GetDefault(ex),
                    _ => GetDefault(aggregate)
                },
                FormatException => new HealthNameIssueViewModel(storable, result, "InvalidItemName".ToLocalized()) { ErrorMessage = "GenerateNewName".ToLocalized() },
                FileHeaderCorruptedException => new HealthFileDataIssueViewModel(storable, result, "IrrecoverableFile".ToLocalized(), isRecoverable: false) { ErrorMessage = "FileHeaderCorrupted".ToLocalized() },
                FileChunksCorruptedException chunksEx => new HealthFileDataIssueViewModel(storable, result, "CorruptedFileChunks".ToLocalized(), chunksEx.CorruptedChunks, isRecoverable: true) { ErrorMessage = "FileHasCorruptedChunks".ToLocalized(chunksEx.CorruptedChunks.Count) },
                CryptographicException => new HealthFileDataIssueViewModel(storable, result, "InvalidFileContents".ToLocalized()) { ErrorMessage = "RegenerateFileContents".ToLocalized() },
                { } ex => GetDefault(ex)
            };

            HealthIssueViewModel GetDefault(Exception ex)
            {
                return new HealthIssueViewModel(storable, result, ex.GetType().Name);
            }
        }

        /// <inheritdoc/>
        public async Task ResolveIssuesAsync(IEnumerable<HealthIssueViewModel> issues, IDisposable contractOrRoot, IVaultHealthService.IssueDelegate? issueDelegate, CancellationToken cancellationToken = default)
        {
            // Sort in the following order:
            /*
                1. Corrupted file contents (HealthFileDataIssueViewModel)
                2. Corrupted folder/file names (HealthNameIssueViewModel) and corrupted Directory ID (HealthDirectoryIssueViewModel) starting with items with most parents
            */
            var sortedIssues = issues
                .OrderByDescending(issue => issue is HealthFileDataIssueViewModel)
                .ThenBy(issue => issue is HealthNameIssueViewModel or HealthDirectoryIssueViewModel)
                .ThenByDescending(issue =>
                {
                    // Get the amount of items
                    var pathComponents = issue.Inner.Id.Split(Path.DirectorySeparatorChar).Length;

                    // We actually prioritize Directory ID issues first, since the children
                    // of the affected directory also need to be renamed in one sweep
                    return issue is HealthDirectoryIssueViewModel ? ++pathComponents : pathComponents;
                })
                .ToArray();

            if (contractOrRoot is IWrapper<FileSystemSpecifics> specificsWrapper)
            {
                foreach (var item in sortedIssues)
                {
                    var result = item switch
                    {
                        // Name issue
                        HealthNameIssueViewModel nameIssue => await HealthHelpers.RepairNameAsync(
                            nameIssue.Inner,
                            specificsWrapper.Inner,
                            FormattingHelpers.SanitizeItemName(nameIssue.ItemName ?? string.Empty, nameIssue.OriginalName),
                            cancellationToken),

                        // Directory ID issue
                        HealthDirectoryIssueViewModel directoryIssue => await HealthHelpers.RepairDirectoryAsync(
                            directoryIssue.Folder ?? throw new ArgumentNullException(nameof(HealthDirectoryIssueViewModel.Folder)),
                            specificsWrapper.Inner.Security,
                            cancellationToken),

                        // File data issue - repair chunks or delete irrecoverable file
                        HealthFileDataIssueViewModel { IsRecoverable: true, File: not null } dataIssue => await HealthHelpers.RepairFileChunksAsync(
                            dataIssue.File,
                            specificsWrapper.Inner.Security,
                            dataIssue.CorruptedChunks,
                            cancellationToken),

                        // Irrecoverable file - delete it
                        HealthFileDataIssueViewModel { IsRecoverable: false, File: not null } dataIssue => await HealthHelpers.DeleteIrrecoverableFileAsync(
                            dataIssue.File,
                            cancellationToken),

                        // Default
                        _ => null
                    };

                    if (result is not null)
                        issueDelegate?.Invoke(item, result);
                }
            }
        }
    }
}
